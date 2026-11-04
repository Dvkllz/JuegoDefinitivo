import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { generateChallenge } from './director.mjs';
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const envPath = path.join(root, '.env.local');
if (fs.existsSync(envPath)) process.loadEnvFile(envPath);
try {
  const result = await generateChallenge({ world: 0, seal: 0, length: 3,
    profile: { skill: 0.25, trials: 0, streak: 0, seen: [0,0,0,0], missed: [0,0,0,0] } },
    { apiKey: process.env.OPENAI_API_KEY, model: process.env.NEBULA_OPENAI_MODEL || 'gpt-4.1-mini' });
  fs.mkdirSync(path.join(root, 'TestResults'), { recursive: true });
  fs.writeFileSync(path.join(root, 'TestResults', 'openai-live-result.json'), JSON.stringify({ ok: true, ...result }, null, 2));
  console.log(JSON.stringify({ ok: true, provider: result.provider, model: result.model, title: result.title, motif: result.motif }));
} catch (error) { console.error(JSON.stringify({ ok: false, error: error.code || 'check_failed' })); process.exitCode = 1; }
