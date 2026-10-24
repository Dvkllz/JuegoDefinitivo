import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawn } from 'node:child_process';
import { startBridge } from './bridge.mjs';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const envPath = path.join(root, '.env.local');
if (fs.existsSync(envPath)) process.loadEnvFile(envPath);
const model = process.env.NEBULA_OPENAI_MODEL || 'gpt-4.1-mini';
const bridge = await startBridge({ apiKey: process.env.OPENAI_API_KEY, model });
const sessionPath = path.join(root, '.nebula-session.json');
// Contains only a temporary loopback token; never the OpenAI API key.
fs.writeFileSync(sessionPath, JSON.stringify({ url: bridge.url, token: bridge.token }), { mode: 0o600 });
console.log(`Nébula: puente local listo. Modelo: ${model}. Máximo 60 generaciones por sesión.`);
if (!process.env.OPENAI_API_KEY) console.log('No hay una clave configurada: el juego utilizará patrones locales.');
let closing = false;
async function close() {
  if (closing) return; closing = true;
  try { const saved = JSON.parse(fs.readFileSync(sessionPath, 'utf8')); if (saved.token === bridge.token) fs.unlinkSync(sessionPath); } catch {}
  await bridge.close();
}
for (const signal of ['SIGINT', 'SIGTERM']) process.once(signal, () => void close());
if (!process.argv.includes('--server-only')) {
  const executable = path.join(root, 'Builds', 'Windows', 'EcosDeNebula.exe');
  if (!fs.existsSync(executable)) { console.error('No se encontró el ejecutable de Unity. Compila la versión Windows.'); await close(); process.exitCode = 1; }
  else {
    const env = { ...process.env, NEBULA_AI_URL: bridge.url, NEBULA_AI_TOKEN: bridge.token };
    delete env.OPENAI_API_KEY; // The child game never receives the provider credential.
    const game = spawn(executable, [], { cwd: path.dirname(executable), env, stdio: 'ignore' });
    game.once('error', async () => { console.error('No se pudo abrir el juego.'); await close(); process.exitCode = 1; });
    game.once('exit', () => void close());
  }
}
