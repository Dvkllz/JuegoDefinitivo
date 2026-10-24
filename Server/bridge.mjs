import http from 'node:http';
import { timingSafeEqual, randomBytes } from 'node:crypto';
import { DirectorError, generateChallenge, validateRequest } from './director.mjs';

export async function startBridge({ apiKey, model = 'gpt-4.1-mini', generate = generateChallenge, maxRequests = 60 } = {}) {
  const token = randomBytes(32).toString('hex');
  let pending = false, used = 0, cooldownUntil = 0;
  const server = http.createServer(async (req, res) => {
    const reply = (status, value) => { if (!res.destroyed) { res.writeHead(status, { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' }); res.end(JSON.stringify(value)); } };
    // Loopback only, no CORS and no browser Origin: untrusted web pages cannot spend the key.
    const supplied = Buffer.from(req.headers.authorization || '');
    const expected = Buffer.from(`Bearer ${token}`);
    if (req.headers.origin || supplied.length !== expected.length || !timingSafeEqual(supplied, expected)) return reply(401, { error: 'unauthorized' });
    if (req.method === 'GET' && req.url === '/health') return reply(200, { ready: Boolean(apiKey), model, used, remaining: Math.max(0, maxRequests - used) });
    if (req.method !== 'POST' || req.url !== '/challenge') return reply(404, { error: 'not_found' });
    if (!apiKey) return reply(503, { error: 'missing_key' });
    if (pending || Date.now() < cooldownUntil) return reply(429, { error: 'cooldown' });
    if (used >= maxRequests) return reply(429, { error: 'session_limit' });
    const controller = new AbortController();
    res.on('close', () => { if (!res.writableEnded) controller.abort(); });
    try {
      if (!String(req.headers['content-type']).startsWith('application/json')) throw new DirectorError('invalid_request', 400);
      let body = '', bytes = 0;
      for await (const chunk of req) {
        bytes += chunk.length;
        if (bytes > 8192) throw new DirectorError('payload_too_large', 413);
        body += chunk.toString('utf8');
      }
      let input;
      try { input = validateRequest(JSON.parse(body)); } catch { throw new DirectorError('invalid_request', 400); }
      // Recheck after reading: two concurrent request bodies must not race the single-flight gate.
      if (pending || used >= maxRequests || Date.now() < cooldownUntil) throw new DirectorError('cooldown', 429);
      pending = true; used++;
      try { reply(200, await generate(input, { apiKey, model, signal: controller.signal })); }
      finally { pending = false; }
    } catch (error) {
      if (!(error instanceof DirectorError)) error = new DirectorError('internal_error');
      if (error.status >= 500) cooldownUntil = Date.now() + 60000;
      reply(error.status, { error: error.code });
    }
  });
  server.requestTimeout = 25000; server.headersTimeout = 10000;
  await new Promise((resolve, reject) => { server.once('error', reject); server.listen(0, '127.0.0.1', resolve); });
  return { server, token, url: `http://127.0.0.1:${server.address().port}`, close: () => new Promise(resolve => { server.close(resolve); server.closeAllConnections(); }) };
}
