// OpenAI lives only in this local process. Never import this file into Unity assets.
export class DirectorError extends Error {
  constructor(code, status = 503) { super(code); this.code = code; this.status = status; }
}

export function validateRequest(data) {
  const integer = (v, lo, hi) => Number.isInteger(v) && v >= lo && v <= hi;
  if (!data || !integer(data.world, 0, 2) || !integer(data.seal, 0, 3) ||
      !integer(data.length, data.world === 2 ? 2 : 3, data.world === 2 ? 3 : 7))
    throw new DirectorError('invalid_request', 400);
  const p = data.profile;
  if (!p || !Number.isFinite(p.skill) || p.skill < 0 || p.skill > 1 ||
      !integer(p.trials, 0, 1000000) || !integer(p.streak, 0, 1000000) ||
      !Array.isArray(p.seen) || !Array.isArray(p.missed) || p.seen.length !== 4 || p.missed.length !== 4 ||
      p.seen.some((n, i) => !integer(n, 0, 10000000) || !integer(p.missed[i], 0, n)))
    throw new DirectorError('invalid_profile', 400);
  return { world: data.world, seal: data.seal, length: data.length,
    profile: { skill: p.skill, trials: p.trials, streak: p.streak, seen: p.seen, missed: p.missed } };
}

export function validateGeneration(data, request) {
  if (!data || !Array.isArray(data.motif) || data.motif.length !== request.length ||
      data.motif.some(n => !Number.isInteger(n) || n < 0 || n > 3) || new Set(data.motif).size < 2)
    throw new DirectorError('invalid_generation');
  for (const [name, length] of [['title', 44], ['message', 140], ['hint', 130]]) {
    if (typeof data[name] !== 'string' || !data[name].trim() || data[name].length > length || /[<>\x00-\x08]/.test(data[name]))
      throw new DirectorError('invalid_generation');
  }
  if (!['orbit', 'wave', 'spiral'].includes(data.constellation)) throw new DirectorError('invalid_generation');
  return { title: data.title, message: data.message, hint: data.hint, motif: data.motif, constellation: data.constellation };
}

export async function generateChallenge(request, { apiKey, model = 'gpt-4.1-mini', fetchImpl = fetch, signal } = {}) {
  if (!apiKey) throw new DirectorError('missing_key');
  request = validateRequest(request);
  const schema = { type: 'object', additionalProperties: false,
    properties: {
      title: { type: 'string', maxLength: 44 }, message: { type: 'string', maxLength: 140 },
      hint: { type: 'string', maxLength: 130 }, constellation: { type: 'string', enum: ['orbit', 'wave', 'spiral'] },
      motif: { type: 'array', minItems: request.length, maxItems: request.length,
        items: { type: 'integer', minimum: 0, maximum: 3 } }
    }, required: ['title', 'message', 'hint', 'constellation', 'motif'] };
  const instructions = `Eres El Tejedor, mentor de Luma en el juego 3D Ecos de Nébula. Crea un reto original y justo.
Todo el texto en español, cálido y breve. Runas: 0 Hoja, 1 Sol, 2 Marea, 3 Estrella.
Mundo 0 Bosque de Jade: el jugador repetirá motif. Mundo 1 Mar de Ámbar: verá motif y responderá al revés.
Mundo 2 Observatorio Violeta: verá motif repetido dos veces y completará una tercera repetición.
Genera exactamente length enteros, al menos dos distintos. Usa agrupaciones, simetrías o alternancia con variedad.
Refuerza las runas con mayor proporción de errores (missed+1)/(seen+4). No inventes métricas ni afirmes detectar emociones.
title nombra este patrón; message es una frase de ambientación/ánimo sin revelar números o el orden.
hint explica la REGLA del mundo sin dar la solución ni otro orden. Nunca cambies la regla, jamás inventes nuevas teclas.
constellation elige un adorno visual: orbit, wave o spiral. No escribas código, enlaces ni instrucciones fuera del juego.`;
  let response;
  try {
    response = await fetchImpl('https://api.openai.com/v1/responses', {
      method: 'POST', headers: { Authorization: `Bearer ${apiKey}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ model, store: false, instructions, input: JSON.stringify(request), max_output_tokens: 650,
        text: { format: { type: 'json_schema', name: 'nebula_challenge', strict: true, schema } } }),
      signal: signal ? AbortSignal.any([signal, AbortSignal.timeout(20000)]) : AbortSignal.timeout(20000)
    });
  } catch { throw new DirectorError(signal?.aborted ? 'cancelled' : 'network_or_timeout'); }
  if (!response.ok) {
    // Do not return provider error bodies, headers, request bodies or credentials to the game/logs.
    const code = response.status === 401 ? 'invalid_key' : response.status === 429 ? 'quota_or_rate_limit' :
      response.status === 403 || response.status === 404 ? 'model_access' : 'provider_unavailable';
    throw new DirectorError(code);
  }
  let raw;
  try { raw = await response.json(); } catch { throw new DirectorError('invalid_generation'); }
  if (raw.status !== 'completed' || !Array.isArray(raw.output)) throw new DirectorError('incomplete_generation');
  const text = raw.output.filter(o => o.type === 'message').flatMap(o => o.content || [])
    .filter(c => c.type === 'output_text').map(c => c.text).join('');
  let result;
  try { result = JSON.parse(text); } catch { throw new DirectorError('invalid_generation'); }
  return { ...validateGeneration(result, request), provider: 'openai', model };
}
