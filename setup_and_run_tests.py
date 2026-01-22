import os
import sys
import subprocess
import glob

DTOMODEL_CSPROJ_CONTENT = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
"""

CSHARP_TEST_CODE = r"""
using NUnit.Framework;
using DTOModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace DialogueEngine.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        // SceneDTO
        [Test]
        public void SceneDTO_Should_Serialize_Prompt_And_Location()
        {
            var scene = new SceneDTO 
            { 
                LocationName = "Stara Fabryka", 
                ScenePrompt = "Słychać maszyny w oddali." 
            };

            string json = JsonConvert.SerializeObject(scene);
            SceneDTO? result = JsonConvert.DeserializeObject<SceneDTO>(json);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.LocationName, Is.EqualTo("Stara Fabryka"));
            Assert.That(result.ScenePrompt, Is.EqualTo("Słychać maszyny w oddali."));
        }

        // ItemDTO
        [Test]
        public void ItemDTO_Should_Serialize_All_Fields()
        {
            var item = new ItemDTO 
            { 
                name = "Mapa", 
                description = "Stara mapa miasta", 
                hints = "Wskazuje ukryte przejście" 
            };

            string json = JsonConvert.SerializeObject(item);
            ItemDTO? result = JsonConvert.DeserializeObject<ItemDTO>(json);

            Assert.That(result.name, Is.EqualTo("Mapa"));
            Assert.That(result.description, Is.EqualTo("Stara mapa miasta"));
            Assert.That(result.hints, Is.EqualTo("Wskazuje ukryte przejście"));
        }

        // NPCRequestDTO
        [Test]
        public void NPCRequestDTO_Should_Serialize_Correctly()
        {
            var request = new NPCRequestDTO
            {
                NPCName = "Detektyw",
                SceneDescription = "Miejsce zbrodni",
                UserText = "Co tu się stało?"
            };
            string json = JsonConvert.SerializeObject(request);
            
            // Sprawdzenie atrybutu [JsonProperty("npcName")]
            Assert.That(json, Does.Contain("\"npcName\":\"Detektyw\"")); 
        }

        // NPCDTO
        [Test]
        public void NPCDTO_Should_Handle_Properties()
        {
            var npc = new NPCDTO { name = "Barman", role = "Sprzedawca", description="Miły", protrait = "img.png" };
            string json = JsonConvert.SerializeObject(npc);
            NPCDTO? result = JsonConvert.DeserializeObject<NPCDTO>(json);
            
            Assert.That(result.name, Is.EqualTo("Barman"));
            Assert.That(result.protrait, Is.EqualTo("img.png"));
        }

        // SettingsDTO
        [Test]
        public void SettingsDTO_Should_Serialize_Volume()
        {
            var settings = new SettingsDTO { Volume = 75 };
            string json = JsonConvert.SerializeObject(settings);
            SettingsDTO? result = JsonConvert.DeserializeObject<SettingsDTO>(json);
            Assert.That(result.Volume, Is.EqualTo(75));
        }

        // EndingDTO
        [Test]
        public void EndingDTO_Should_Serialize_Correctly()
        {
            var ending = new EndingDTO
            {
                AccusedName = "Podejrzany",
                Description = "Zakończenie gry",
                IsMurderer = true
            };
            string json = JsonConvert.SerializeObject(ending);
            EndingDTO? result = JsonConvert.DeserializeObject<EndingDTO>(json);
            
            Assert.That(result.AccusedName, Is.EqualTo("Podejrzany"));
            Assert.That(result.IsMurderer, Is.True);
        }

        // ScenesScriptDTO
        [Test]
        public void ScenesScriptDTO_Complex_Graph_Test()
        {
            var scene = new SceneScriptDTO
            {
                Name = "Kitchen",
                Npcs = new NPCDTO[] { new NPCDTO { name = "Cook" } },
                Items = new ItemDTO[] { } 
            };
            var script = new ScenesScriptDTO
            {
                Scenes = new SceneScriptDTO[] { scene },
                Endings = new EndingDTO[] { }
            };
            
            string json = JsonConvert.SerializeObject(script);
            ScenesScriptDTO? result = JsonConvert.DeserializeObject<ScenesScriptDTO>(json);
            
            Assert.That(result.Scenes[0].Name, Is.EqualTo("Kitchen"));
            Assert.That(result.Scenes[0].Npcs[0].name, Is.EqualTo("Cook"));
        }

        // Znaki specjalne
        [Test]
        public void EdgeCase_SpecialCharacters_And_PolishLetters()
        {
            var response = new NPCResponseDTO 
            { 
                Speech = "Zażółć gęślą jaźń.", 
                Action = "Mówi: \"Test\""
            };
            string json = JsonConvert.SerializeObject(response);
            NPCResponseDTO? restored = JsonConvert.DeserializeObject<NPCResponseDTO>(json);
            
            Assert.That(restored.Speech, Is.EqualTo("Zażółć gęślą jaźń."));
        }

        // Null / Pusta Tablica
        [Test]
        public void EdgeCase_Null_vs_Empty_Arrays()
        {
            string jsonWithNulls = "{\"Name\": \"Pustka\", \"Npcs\": null, \"Items\": null}";
            SceneScriptDTO? result = JsonConvert.DeserializeObject<SceneScriptDTO>(jsonWithNulls);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Npcs, Is.Null); 
        }
    }
}
"""

def print_step(message):
    print(f"\n{'='*60}")
    print(f"--> {message}")
    print(f"{'='*60}")

def run_command(command, cwd=None, ignore_error=False):
    try:
        subprocess.check_call(command, cwd=cwd, shell=True)
        return True
    except subprocess.CalledProcessError:
        if not ignore_error:
            print(f"[BŁĄD] Nie zadziałała komenda {command}")
            sys.exit(1)
        return False

def find_file(name, search_path):
    for root, dirs, files in os.walk(search_path):
        if name in files:
            return os.path.join(root, name)
    return None

def ensure_dtomodel_exists(dialogue_engine_path):
    print("Sprawdzanie projektu DTOModel...")
    csproj_path = find_file("DTOModel.csproj", dialogue_engine_path)
    if csproj_path:
        print(f"Znaleziono projekt: {csproj_path}")
        return csproj_path
    
    dtomodel_dir = os.path.join(dialogue_engine_path, "DTOModel")
    if not os.path.exists(dtomodel_dir):
        found_dirs = glob.glob(os.path.join(dialogue_engine_path, "**", "DTOModel"), recursive=True)
        if found_dirs:
            dtomodel_dir = found_dirs[0]
        else:
            print("[BŁĄD] Nie znaleziono folderu DTOModel.")
            sys.exit(1)

    new_csproj_path = os.path.join(dtomodel_dir, "DTOModel.csproj")
    with open(new_csproj_path, "w", encoding="utf-8") as f:
        f.write(DTOMODEL_CSPROJ_CONTENT)
    return new_csproj_path

def setup_python():
    print_step("Konfiguracja środowiska Python")
    deps = ["pytest", "fastapi", "httpx", "requests"]
    run_command(f"{sys.executable} -m pip install {' '.join(deps)}")

def setup_csharp(root_dir):
    print_step("Konfiguracja środowiska C#")
    dialogue_engine_path = os.path.join(root_dir, "DialogueEngine")
    if not os.path.exists(dialogue_engine_path):
        print(f"[BŁĄD] Brak folderu {dialogue_engine_path}.")
        sys.exit(1)

    dto_csproj_path = ensure_dtomodel_exists(dialogue_engine_path)
    test_project_dir = os.path.join(dialogue_engine_path, "DialogueEngine.Tests")
    
    if not os.path.exists(test_project_dir):
        print("Tworzenie projektu testowego C#...")
        run_command(f"dotnet new nunit -o \"{test_project_dir}\"")
    
    run_command("dotnet add package Newtonsoft.Json", cwd=test_project_dir)
    run_command(f"dotnet add reference \"{dto_csproj_path}\"", cwd=test_project_dir)

    test_file_path = os.path.join(test_project_dir, "SerializationTests.cs")
    print(f"Aktualizacja kodu testów w: {test_file_path}")
    with open(test_file_path, "w", encoding="utf-8") as f:
        f.write(CSHARP_TEST_CODE)

    default_test = os.path.join(test_project_dir, "UnitTest1.cs")
    if os.path.exists(default_test): os.remove(default_test)

    return test_project_dir

def run_tests(root_dir, csharp_test_dir):
    all_passed = True
    
    print_step("Uruchamianie testów Python")
    python_test_path = os.path.join(root_dir, "AI_testowanie", "tests", "test_api.py")
    if os.path.exists(python_test_path):
        if not run_command(f"{sys.executable} -m pytest \"{python_test_path}\" -v", ignore_error=True):
            all_passed = False
    else:
        print(f"[WARN] Brak pliku {python_test_path}")

    print_step("Uruchamianie testów C#")
    if not run_command("dotnet test", cwd=csharp_test_dir, ignore_error=True):
        all_passed = False

    return all_passed

if __name__ == "__main__":
    current_dir = os.getcwd()
    try:
        subprocess.check_output(["dotnet", "--version"])
    except:
        print("[BŁĄD] Brak zainstalowanego .NET SDK.")
        sys.exit(1)

    setup_python()
    csharp_path = setup_csharp(current_dir)
    success = run_tests(current_dir, csharp_path)
    
    print_step("PODSUMOWANIE")
    if success:
        print("WSZYSTKIE TESTY ZAKOŃCZONE POMYŚLNIE")
        sys.exit(0)
    else:
        print("WYSTĄPIŁY BŁĘDY")
        sys.exit(1)