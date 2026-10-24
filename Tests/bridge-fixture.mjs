// Integration fixture ONLY. No provider request is made; excluded from distribution.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { startBridge } from '../Server/bridge.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const sessionPath=path.join(root,'.nebula-session.json');
const previous=fs.existsSync(sessionPath)?fs.readFileSync(sessionPath):null;
let done=false,timer;
const bridge=await startBridge({apiKey:'mock-only-not-a-key',generate:async(request)=>{
  clearTimeout(timer);timer=setTimeout(close,12000);
  return {provider:'openai',model:'MOCK-CONTRACT-TEST',motif:Array.from({length:request.length},(_,i)=>i%4),title:'Prueba de contrato',
    message:'Respuesta simulada exclusivamente para pruebas.',hint:'Sigue la regla del santuario.',constellation:'wave'};
}});
fs.writeFileSync(sessionPath,JSON.stringify({url:bridge.url,token:bridge.token}));
console.log('Fixture ready. No real OpenAI requests.');
async function close(){
  if(done)return;done=true;clearTimeout(timer);
  try{if(JSON.parse(fs.readFileSync(sessionPath,'utf8')).token===bridge.token){
    if(previous)fs.writeFileSync(sessionPath,previous);else fs.unlinkSync(sessionPath);
  }}catch{}
  await bridge.close();
}
timer=setTimeout(close,120000);process.once('SIGTERM',close);process.once('SIGINT',close);
