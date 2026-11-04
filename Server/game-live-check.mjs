// Explicit opt-in live integration test. Uses one provider request; preserves game saves.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawn } from 'node:child_process';
import { startBridge } from './bridge.mjs';
import { generateChallenge } from './director.mjs';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const envPath = path.join(root, '.env.local');
if (fs.existsSync(envPath)) process.loadEnvFile(envPath);
const unity = process.env.NEBULA_UNITY_PATH || 'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe';
const model = process.env.NEBULA_OPENAI_MODEL || 'gpt-4.1-mini';
let bridge;
try {
  if (!process.env.OPENAI_API_KEY) throw new Error('missing_key');
  if (!fs.existsSync(unity)) throw new Error('unity_not_found');
  fs.mkdirSync(path.join(root, 'TestResults'), { recursive: true });
  let generated;
  bridge = await startBridge({ apiKey: process.env.OPENAI_API_KEY, model, maxRequests: 1,
    generate: async (request, options) => {
      const result = await generateChallenge(request, options);
      generated = { language: request.language, ...result };
      return result;
    }
  });
  const env = { ...process.env, NEBULA_AI_URL: bridge.url, NEBULA_AI_TOKEN: bridge.token };
  delete env.OPENAI_API_KEY;
  const exitCode = await new Promise((resolve, reject) => {
    const child = spawn(unity, ['-batchmode', '-projectPath', root,
      '-executeMethod', 'EcosDeNebula.Editor.NebulaSmokeTest.ApiOnce',
      '-logFile', path.join(root, 'TestResults', 'Unity-live-api.log')],
    { cwd: root, env, windowsHide: true, stdio: 'ignore' });
    const timeout = setTimeout(() => { child.kill(); reject(new Error('unity_timeout')); }, 180000);
    child.once('error', () => { clearTimeout(timeout); reject(new Error('unity_start_failed')); });
    child.once('exit', code => { clearTimeout(timeout); resolve(code); });
  });
  if (exitCode !== 0 || !generated) throw new Error('unity_integration_failed');
  const receipt = { ok: true, checkedAt: new Date().toISOString(), providerRequests: 1,
    unity: 'validated playable pattern and local fallback', ...generated };
  fs.writeFileSync(path.join(root, 'TestResults', 'Unity-live-api-generation.json'), JSON.stringify(receipt, null, 2));
  console.log(JSON.stringify({ ok: true, unity: true, providerRequests: 1,
    model: receipt.model, language: receipt.language, title: receipt.title, motif: receipt.motif }));
} catch (error) {
  const safeCodes = ['missing_key', 'unity_not_found', 'unity_timeout', 'unity_start_failed', 'unity_integration_failed'];
  console.error(JSON.stringify({ ok: false, error: safeCodes.includes(error.message) ? error.message : 'live_check_failed' }));
  process.exitCode = 1;
} finally {
  if (bridge) await bridge.close();
}
