using System;
using System.IO;
using EcosDeNebula;

public static class LocalizationTests
{
    static int checks;
    static void Equal(string expected,string actual)
    {
        checks++;
        if(expected!=actual)throw new Exception("Expected: "+expected+"; actual: "+actual);
    }
    public static int Main()
    {
        try
        {
            string catalogue=File.ReadAllText("Assets/EcosDeNebula/Resources/NebulaEnglish.txt");
            NebulaLocalization.LoadEnglish(catalogue);
            foreach(string line in catalogue.Replace("\r","").Split('\n'))
            {
                if(line.Length==0||line.StartsWith("#"))continue;
                int separator=line.IndexOf('|');
                string source=line.Substring(0,separator).Replace("\\n","\n").Replace("\\s"," ");
                string translated=line.Substring(separator+1).Replace("\\n","\n").Replace("\\s"," ");
                NebulaLocalization.SetLanguage("es");Equal(source,NebulaLocalization.Text(source));
                NebulaLocalization.SetLanguage("en");Equal(translated,NebulaLocalization.Text(source));
            }
            Equal("LEVEL 5   /   210 ESSENCE   /   3 ACHIEVEMENTS",NebulaLocalization.Text("NIVEL 5   /   210 ESENCIA   /   3 LOGROS"));
            Equal("GUARDIAN · HEALTH 3 / 3",NebulaLocalization.Text("GUARDIÁN · VIDA 3 / 3"));
            Equal("SANCTUARY 01 / 03",NebulaLocalization.Text("SANTUARIO 01 / 03"));
            Equal("5 flashes = 5 answers. Enemies arrive when you respond.",NebulaLocalization.Text("5 destellos = 5 respuestas. Los enemigos entran al responder."));
            Equal("Wrong code: expected rune 4. The expedition restarts from the beginning.",NebulaLocalization.Text("Código incorrecto: se esperaba la runa 4. La expedición vuelve al principio."));
            Equal("Luma has restored the memory of Amber Sea. The next sanctuary holds a new rule.",NebulaLocalization.Text("Luma ha devuelto la memoria a Mar de Ámbar. El siguiente santuario guarda una nueva regla."));
            Equal("Achievement: First awakening · +15 essence",NebulaLocalization.Text("Logro: Primer despertar · +15 esencia"));
            Equal("THE WEAVER · AI",NebulaLocalization.Text("EL TEJEDOR · IA"));
            Equal("REPEAT THE FULL CODE IN REVERSE ORDER.",NebulaLocalization.Text("REPITE EL CÓDIGO COMPLETO EN ORDEN INVERSO."));
            Equal("SOLVED 123",NebulaLocalization.Text("SOLVED 123"));
            Equal("New generated prose.",NebulaLocalization.Text("New generated prose."));
            Equal(null,NebulaLocalization.Text(null));Equal("",NebulaLocalization.Text(""));
            for(int i=0;i<600;i++)Equal(i+" sparks",NebulaLocalization.Text(i+" destellos"));
            NebulaLocalization.SetLanguage("es");Equal("300 destellos",NebulaLocalization.Text("300 destellos"));
            NebulaLocalization.SetLanguage("en");Equal("300 sparks",NebulaLocalization.Text("300 destellos"));
            NebulaLocalization.SetLanguage("invalid");Equal("es",NebulaLocalization.Language);
            Console.WriteLine("PASS: "+checks+" localization checks; no network calls.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}
