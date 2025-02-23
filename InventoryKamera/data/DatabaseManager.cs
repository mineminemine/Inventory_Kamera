﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InventoryKamera
{
    public class DatabaseManager
	{
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		private string _listdir = @".\inventorylists\";
		private readonly string versionJson = "versions.json";

		public string ListsDir
		{
			get
			{
				Directory.CreateDirectory(_listdir);
				return _listdir;
			}
			set { _listdir = value; }
		}

		private const string WeaponsJson = "weapons.json";
		private const string ArtifactsJson = "artifacts.json";
		private const string CharactersJson = "characters.json";
		private const string DevMaterialsJson = "devmaterials.json";
		private const string MaterialsJson = "materials.json";
		private const string MaterialsCompleteJson = "allmaterials.json";

		// This is the best place I think we can find easily accessible and up-to-date lists of information
		private const string CharactersURL = "https://raw.githubusercontent.com/Dimbreath/GenshinData/master/ExcelBinOutput/AvatarExcelConfigData.json";
		private const string ConstellationsURL = "https://raw.githubusercontent.com/Dimbreath/GenshinData/master/ExcelBinOutput/AvatarTalentExcelConfigData.json";
		private const string SkillsURL = "https://raw.githubusercontent.com/Dimbreath/GenshinData/master/ExcelBinOutput/AvatarSkillExcelConfigData.json";
		private const string ArtifactsURL = "https://raw.githubusercontent.com/Dimbreath/GenshinData/master/ExcelBinOutput/DisplayItemExcelConfigData.json";
		private const string WeaponsURL = "https://raw.githubusercontent.com/Dimbreath/GenshinData/master/ExcelBinOutput/WeaponExcelConfigData.json";
		private const string MaterialsURL = "https://raw.githubusercontent.com/Dimbreath/GenshinData/master/ExcelBinOutput/MaterialExcelConfigData.json";
		
		private const string MappingsURL = "https://raw.githubusercontent.com/Dimbreath/GenshinData/master/TextMap/TextMapEN.json";

		private Dictionary<string, string> Mappings = new Dictionary<string, string>();

		private int updaters = 0;

		#region Progress Variables

		private int _weapons_todo;
		private int _artifactsTodo;
		private int _charactersTodo;
		private int _devTodo;
		private int _materialsTodo;

		private int _weaponsCompleted;
		private int _artifactsCompleted;
		private int _charactersCompleted;
		private int _devCompleted;
		private int _materialsCompleted;

		private int _completed;
		private int _todo;


        public int TotalCompleted
		{
			get { return _completed; }
			private set { _completed = value; }
		}

		public int TotalTodo
		{
			get { return _todo; }
			private set { _todo = value; }
		}

		public int WeaponsTodo
		{
			get { return _weapons_todo; }
			set { _weapons_todo = value; _todo += value; }
		}

		public int WeaponsCompleted
		{
			get { return _weaponsCompleted; }
			set { _weaponsCompleted = value; _completed += value; }
		}

		public int ArtifactsTodo
		{
			get { return _artifactsTodo; }
			set { _artifactsTodo = value; _todo += value; }
		}

		public int ArtifactsCompleted
		{
			get { return _artifactsCompleted; }
			set { _artifactsCompleted = value; _completed += value; }
		}

		public int CharactersTodo
		{
			get { return _charactersTodo; }
			set { _charactersTodo = value; _todo += value; }
		}

		public int CharactersCompleted
		{
			get { return _charactersCompleted; }
			set { _charactersCompleted = value; _completed += value; }
		}

		public int DevMaterialsTodo
		{
			get { return _devTodo; }
			set { _devTodo = value; _todo += value; }
		}

		public int DevMaterialsCompleted
		{
			get { return _devCompleted; }
			set { _devCompleted = value; _completed += value; }
		}

		public int MaterialsTodo
		{
			get { return _materialsTodo; }
			set { _materialsTodo = value; _todo += value; }
		}

		public int MaterialsCompleted
		{
			get { return _materialsCompleted; }
			set { _materialsCompleted = value; _completed += value; }
		}

		#endregion Progress Variables

		internal Dictionary<string, string> localVersions;
		internal Version GameVersion = new Version();
		internal Version RemoteVersion;

		public DatabaseManager()
		{
			Directory.CreateDirectory(ListsDir);

			if (!File.Exists(ListsDir + versionJson))
			{
				File.Create(ListsDir + versionJson).Close();
			}			
			RemoteVersion = new Version(Properties.Settings.Default.RemoteVersion);
			localVersions = JToken.Parse(LoadJsonFromFile(versionJson)).ToObject<Dictionary<string, string>>();
			if (localVersions.Keys.Count < 6) Properties.Settings.Default.LastUpdateCheck = DateTime.MinValue;
			if (!UpdateAvailable())
            {
				GameVersion = new Version(localVersions["characters"]);
            }
        }

        private void LoadMappings()
		{
			lock (Mappings)
			{
				if (Mappings.Count == 0)
				{
					Mappings = JObject.Parse(LoadJsonFromURLAsync(MappingsURL))
										 .ToObject<Dictionary<string, string>>()
										 .Where(e => !string.IsNullOrWhiteSpace(e.Value)) // Remove any mapping with empty text
										 .ToDictionary(i => i.Key, i => i.Value);
				}
			}
		}

		private bool ReleaseMappings()
		{
			if (Interlocked.CompareExchange(ref updaters, 0, 0) == 0)
			{
				Mappings = new Dictionary<string, string>();
				Logger.Info("Mappings released");
				return true;
			}
			return false;
		}

		public Dictionary<string, JObject> LoadCharacters()
		{
			return GetList(ListType.Characters).ToObject<Dictionary<string, JObject>>();
		}

		public Dictionary<string, string> LoadWeapons()
		{
			return GetList(ListType.Weapons).ToObject<Dictionary<string, string>>();
		}

		public Dictionary<string, string> LoadArtifacts()
		{
			return GetList(ListType.Artifacts).ToObject<Dictionary<string, string>>();
		}

		public Dictionary<string, string> LoadMaterials()
		{
			return GetList(ListType.Materials).ToObject<Dictionary<string, string>>();
		}

		public Dictionary<string, string> LoadDevMaterials()
		{
			return GetList(ListType.CharacterDevelopmentItems).ToObject<Dictionary<string, string>>();
		}

		public Dictionary<string, string> LoadAllMaterials()
		{
			return GetList(ListType.AllMaterials).ToObject<Dictionary<string, string>>();
		}

		private JToken GetList(ListType list)
		{
			string file = "";
			switch (list)
			{
				case ListType.Weapons:
					file = WeaponsJson;
					break;

				case ListType.Artifacts:
					file = ArtifactsJson;
					break;

				case ListType.Characters:
					file = CharactersJson;
					break;

				case ListType.CharacterDevelopmentItems:
					file = DevMaterialsJson;
					break;

				case ListType.Materials:
					file = MaterialsJson;
					break;

				case ListType.AllMaterials:
					file = MaterialsCompleteJson;
					break;

				default:
					break;
			}

			if (!File.Exists(ListsDir + file)) throw new FileNotFoundException($"Data file does not exist for {list}.");
			string json = LoadJsonFromFile(file);
			if (json == "{}") throw new FormatException($"Data file for {list} is invalid. Please try running the auto updater and try again.");
			return JToken.Parse(json);
		}

		public UpdateStatus UpdateAllLists(bool @new = false, bool force = false)
		{
			UpdateStatus overallStatus = UpdateStatus.Success;

			if (@new)
			{
				Properties.Settings.Default.LastUpdateCheck = DateTime.MinValue;
			}

			var lists = Enum.GetValues(typeof(ListType)).Cast<ListType>().ToList();

            lists.RemoveAll(e => e == ListType.CharacterDevelopmentItems || e == ListType.Materials);

            lists.AsParallel().ForAll(e =>
            {
                var status = UpdateList(e, @new, force);
                overallStatus = overallStatus == UpdateStatus.Fail || status == UpdateStatus.Fail ? UpdateStatus.Fail : status;
                overallStatus = overallStatus == UpdateStatus.Success || status == UpdateStatus.Success ? UpdateStatus.Success : status;
            });

            return overallStatus;
		}

		private UpdateStatus UpdateList(ListType list, bool @new = false, bool force = false)
		{
			Interlocked.Increment(ref updaters);
			LoadMappings();
			UpdateStatus status = UpdateStatus.Success;
            

			Logger.Info("Updating {0}", list);
			switch (list)
			{
				case ListType.Weapons:
					status = UpdateWeapons(@new, force);
					break;

				case ListType.Artifacts:
					status = UpdateArtifacts(@new, force);
					break;

				case ListType.Characters:
					status = UpdateCharacters(@new, force);
					break;

				case ListType.CharacterDevelopmentItems:
					status = UpdateDevItems(@new, force);
					break;

				case ListType.Materials:
					status = UpdateMaterials(@new, force);
					break;

				case ListType.AllMaterials:
					status = UpdateAllMaterials(@new, force);
					break;

				default:
					break;
			}

			Interlocked.Decrement(ref updaters);
			Logger.Info("Finished updating {0}", list);
			return status;
		}

		private UpdateStatus UpdateCharacters(bool @new = false, bool force = false)
		{
			if (@new)
			{
                lock (localVersions)
                {
					localVersions.Remove("characters");
					SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
					File.Delete(ListsDir + CharactersJson);
                }
			}

			try { if (!UpdateAvailable(ListType.Characters, force)) return UpdateStatus.Skipped; }
			catch (Exception e)
			{
				Logger.Error(e, "Could not get check for updates when updating characters. Trying again in an hour should be fine.");
				return UpdateStatus.Fail;
			}
			
			LoadMappings();
			

			try
			{
				Dictionary<string, JObject> data = JToken.Parse( LoadJsonFromFile(CharactersJson)).ToObject<Dictionary<string, JObject>>();

				List<JObject> characters = JArray.Parse(LoadJsonFromURLAsync(CharactersURL)).ToObject<List<JObject>>();
				List<JObject> constellations = JArray.Parse(LoadJsonFromURLAsync(ConstellationsURL)).ToObject<List<JObject>>();
				List<JObject> skills =JArray.Parse(LoadJsonFromURLAsync(SkillsURL)).ToObject<List<JObject>>();

				// Only playable characters have this key. NPCs don't.
				characters.RemoveAll(character => !character.ContainsKey("useType")
												|| character["useType"].ToString() != "AVATAR_FORMAL");
				CharactersTodo = characters.Count;
				Logger.Debug("Added {_charactersTodo} characters. Total {TotalTodo}", _charactersTodo, TotalTodo);

				foreach (var character in characters)
				{
					try
					{
						string name = Mappings[character["nameTextMapHash"].ToString()].ToString();
						string PascalCase = CultureInfo.GetCultureInfo("en").TextInfo.ToTitleCase(name);
						string nameGOOD = Regex.Replace(PascalCase, @"[\W]", string.Empty);
						string nameKey = nameGOOD.ToLower();

						if (!data.ContainsKey(nameKey))
						{
							// Some characters have different internal names.
							// Ex: Jean -> Qin, Yanfei -> Feiyan, etc.
							name = character["iconName"].ToString().Split('_').Last(); // UI_AvatarIcon_[Qin] -> Qin

							name = name.ToLower() == "PlayerBoy".ToLower() || name.ToLower() == "PlayerGirl".ToLower() ? "A" : name; // The name suddenly switches to "A" for travelers

							var skill = skills.Where(entry => entry["skillIcon"].ToString().Contains($"Skill_S_{name}")).First()["nameTextMapHash"].ToString();
							skill = Mappings[skill].ToString();

							// The skill/burst name is always mentioned in the constellation's description so we'll check for it
							var constellation = constellations.Where(entry => entry["icon"].ToString().Contains(name)).ElementAt(2)["descTextMapHash"].ToString();

							var constOrder = new JArray();

							constellation = Mappings[constellation].ToString();
							if (constellation.Contains(skill))
							{
								constOrder.Add("skill");
								constOrder.Add("burst");
							}
							else
							{
								constOrder.Add("burst");
								constOrder.Add("skill");
							}

							var archetype = character["weaponType"].ToString();
							WeaponType weaponType;
							if (archetype.Contains("SWORD_ONE_HAND")) weaponType = WeaponType.Sword;
							else if (archetype.Contains("CLAYMORE")) weaponType = WeaponType.Claymore;
							else if (archetype.Contains("POLE")) weaponType = WeaponType.Polearm;
							else if (archetype.Contains("BOW")) weaponType = WeaponType.Bow;
							else if (archetype.Contains("CATALYST")) weaponType = WeaponType.Catalyst;
							else throw new IndexOutOfRangeException($"{name} uses unknown weapon type {archetype}");

							var value = new JObject
							{
								{ "GOOD", nameGOOD },
								{ "ConstellationOrder", constOrder },
								{ "WeaponType",  (int)weaponType }
							};

							data.Add(nameKey, value);
						}
						++CharactersCompleted;
					}
					catch (Exception ex) { Logger.Warn(ex); }
				}

				SaveJson(JsonConvert.SerializeObject(data), CharactersJson);
				localVersions["characters"] = RemoteVersion.ToString();
				SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
			}
			catch (Exception ex) { Logger.Warn(ex); return UpdateStatus.Fail; }
			return UpdateStatus.Success;
		}

		private UpdateStatus UpdateWeapons(bool @new = false, bool force = false)
		{
			if (@new)
			{
                lock (localVersions)
                {
					localVersions.Remove("weapons");
					SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
					File.Delete(ListsDir + WeaponsJson);
                }
			}

			try { if (!UpdateAvailable(ListType.Weapons, force)) return UpdateStatus.Skipped; }
			catch (Exception e)
			{
				Logger.Error(e, "Could not get check for updates when updating weapons. Trying again in an hour should be fine.");
				return UpdateStatus.Fail;
			}

			LoadMappings();

			try
			{
				Dictionary<string,string> data = JToken.Parse(LoadJsonFromFile(WeaponsJson)).ToObject<Dictionary<string,string>>();
				List<JObject> weapons = JArray.Parse(LoadJsonFromURLAsync(WeaponsURL)).ToObject<List<JObject>>();
				weapons.RemoveAll(weapon => !weapon.ContainsKey("nameTextMapHash"));
				WeaponsTodo = weapons.Count;
				Logger.Debug("Added {_weapons_todo} weapons. Total {TotalTodo}", _weapons_todo, TotalTodo);

				foreach (var weapon in weapons)
				{
					try
					{
						if (Mappings.ContainsKey(weapon["nameTextMapHash"].ToString()))
						{
							var name = Mappings[weapon["nameTextMapHash"].ToString()];
							string PascalCase = CultureInfo.GetCultureInfo("en").TextInfo.ToTitleCase(name); // Dull Blade
							string nameGOOD = Regex.Replace(PascalCase, @"[\W]", string.Empty);              // DullBlade
							string nameKey = nameGOOD.ToLower();                                             // dullblade

							if (!data.ContainsKey(nameKey))
							{
								data.Add(nameKey, nameGOOD);
							}
						}
						else Logger.Warn("Weapon hash {0} not found in Mappings. It's likely unreleased.", weapon["nameTextMapHash"].ToString());
						++WeaponsCompleted;
					}
					catch (Exception ex) { Logger.Warn(ex, weapon["nameTextMapHash"].ToString()); }

				}

				SaveJson(JsonConvert.SerializeObject(data), WeaponsJson);
				localVersions["weapons"] = RemoteVersion.ToString();
				SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
			}
			catch (Exception ex) { Logger.Warn(ex); return UpdateStatus.Fail; }
			return UpdateStatus.Success;
		}

		private UpdateStatus UpdateArtifacts(bool @new = false, bool force = false)
		{
			if (@new)
			{
                lock (localVersions)
                {
					localVersions.Remove("artifacts");
					SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
					File.Delete(ListsDir + ArtifactsJson);
}
}

			try { if (!UpdateAvailable(ListType.Artifacts, force)) return UpdateStatus.Skipped; }
			catch (Exception e)
			{
				Logger.Error(e, "Could not get check for updates when updating artifacts. Trying again in an hour should be fine.");
				return UpdateStatus.Fail;
			}

			LoadMappings();

			try
			{
				Dictionary<string, string> data = JToken.Parse(LoadJsonFromFile(ArtifactsJson)).ToObject < Dictionary < string, string > >();
				List<JObject> artifacts = JArray.Parse(LoadJsonFromURLAsync(ArtifactsURL)).ToObject<List<JObject>>();
				artifacts.RemoveAll(artifact => artifact.TryGetValue("icon", out var icon) && !icon.ToString().Contains("RelicIcon"));

				ArtifactsTodo = artifacts.Count;
				Logger.Debug("Added {_artifactsTodo} artifacts. Total {TotalTodo}", _artifactsTodo, TotalTodo);

				foreach (var artifact in artifacts)
				{
					try
					{
                        if (Mappings.ContainsKey(artifact["nameTextMapHash"].ToString()))
                        {
							var name = Mappings[artifact["nameTextMapHash"].ToString()];
							string PascalCase = CultureInfo.GetCultureInfo("en").TextInfo.ToTitleCase(name);   // Archaic Petra
							string nameGOOD = Regex.Replace(PascalCase, @"[\W]", string.Empty);				// ArchaicPetra
							string nameKey = nameGOOD.ToLower();											// archaicpetra

							if (!data.ContainsKey(nameKey))
							{
								data.Add(nameKey, nameGOOD);
							}
						}
						++ArtifactsCompleted;
					}
					catch (Exception ex) { Logger.Warn(ex); }

				}

				SaveJson(JsonConvert.SerializeObject(data), ArtifactsJson);
				localVersions["artifacts"] = RemoteVersion.ToString();
				SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
			}
			catch (Exception ex) { Logger.Warn(ex); return UpdateStatus.Fail; }
			return UpdateStatus.Success;
		}

		private UpdateStatus UpdateDevItems(bool @new = false, bool force = false)
		{
			if (@new)
			{
                lock (localVersions)
                {
					localVersions.Remove("devmaterials");
					SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
					File.Delete(ListsDir + DevMaterialsJson);
                }
			}

			try { if (!UpdateAvailable(ListType.CharacterDevelopmentItems, force)) return UpdateStatus.Skipped; }
			catch (Exception e)
			{
				Logger.Error(e, "Could not get check for updates when updating character development items. Trying again in an hour should be fine.");
				return UpdateStatus.Fail;
			}

			LoadMappings();

			try
			{
				var categories = new List<string>()
				{
					"MATERIAL_EXP_FRUIT",
					"MATERIAL_AVATAR_MATERIAL",
				};

				Dictionary<string, string> data = JToken.Parse(LoadJsonFromFile(DevMaterialsJson)).ToObject < Dictionary < string, string > >();
				List<JObject> materials = JArray.Parse(LoadJsonFromURLAsync(MaterialsURL)).ToObject<List<JObject>>();
				materials.RemoveAll(material => !material.ContainsKey("materialType") || !categories.Contains(material["materialType"].ToString()));
				DevMaterialsTodo = materials.Count;
				Logger.Debug("Added {_devTodo} dev materials. Total {TotalTodo}", _devTodo, TotalTodo);

				foreach (var material in materials)
				{
					try
					{
						if (Mappings.ContainsKey(material["nameTextMapHash"].ToString()))  
						{
							var name = Mappings[material["nameTextMapHash"].ToString()];
							string PascalCase = CultureInfo.GetCultureInfo("en").TextInfo.ToTitleCase(name);  // Hero's Wit
							string nameGOOD = Regex.Replace(PascalCase, @"[\W]", string.Empty);				  // HerosWit
							string nameKey = nameGOOD.ToLower();											  // heroswit

							if (!data.ContainsKey(nameKey))
							{
								data.Add(nameKey, nameGOOD);
							}
						}
						else Logger.Warn("Material hash {0} not found in Mappings. It's likely unreleased.", material["nameTextMapHash"].ToString());
					}
					catch (Exception ex) { Logger.Warn(ex); }
					++DevMaterialsCompleted;
				}

				SaveJson(JsonConvert.SerializeObject(data), DevMaterialsJson);
				localVersions["devmaterials"] = RemoteVersion.ToString();
				SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
			}
			catch (Exception ex) { Logger.Warn(ex); return UpdateStatus.Fail; }
			return UpdateStatus.Success;
		}

		private UpdateStatus UpdateMaterials(bool @new = false, bool force = false)
		{
			if (@new)
			{
                lock (localVersions)
                {
					localVersions.Remove("materials");
					SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
					File.Delete(ListsDir + MaterialsJson);
				}
			}

            try { if (!UpdateAvailable(ListType.Materials, force)) return UpdateStatus.Skipped; }
            catch (Exception e)
            {
				Logger.Error(e, "Could not get check for updates when updating materials. Trying again in an hour should be fine.");
				return UpdateStatus.Fail;
            }


			LoadMappings();

			try
			{
				var categories = new List<string>()
				{
					"MATERIAL_EXCHANGE",
					"MATERIAL_WOOD",
					"MATERIAL_FISH_BAIT",
					"MATERIAL_RELIQUARY_MATERIAL",  // Artifact sanctifying items
					"MATERIAL_WEAPON_EXP_STONE",    // Enhancement ores
				};

				Dictionary<string, string> data = JToken.Parse(LoadJsonFromFile(MaterialsJson)).ToObject < Dictionary < string, string > >();
				List<JObject> materials = JArray.Parse(LoadJsonFromURLAsync(MaterialsURL)).ToObject<List<JObject>>();
				materials.RemoveAll(material => !material.ContainsKey("materialType") || !categories.Contains(material["materialType"].ToString()));
				MaterialsTodo = materials.Count;
				Logger.Debug("Added {_materialsTodo} materials. Total {TotalTodo}", _materialsTodo, TotalTodo);

				foreach (var material in materials)
				{
					try
					{
						if (Mappings.ContainsKey(material["nameTextMapHash"].ToString())) //Mappings.ContainsKey(weapon["NameTextMapHash"].ToString())
						{
							var name = Mappings[material["nameTextMapHash"].ToString()];
							string PascalCase = CultureInfo.GetCultureInfo("en").TextInfo.ToTitleCase(name);  // Iron Chunk
							string nameGOOD = Regex.Replace(PascalCase, @"[\W]", string.Empty);				  // IronChunk
							string nameKey = nameGOOD.ToLower();											  // ironchunk

							if (!data.ContainsKey(nameKey))
							{
								data.Add(nameKey, nameGOOD);
							}
							Interlocked.Increment(ref _completed);
						}
						else Logger.Warn("Material hash {0} not found in Mappings. It's likely unreleased.", material["nameTextMapHash"].ToString());
					}
					catch (Exception ex) { Logger.Warn(ex); }
					++MaterialsCompleted;
				}

				SaveJson(JsonConvert.SerializeObject(data), MaterialsJson);
				localVersions["materials"] = RemoteVersion.ToString();
				SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
			}
			catch (Exception ex) { Logger.Warn(ex); return UpdateStatus.Fail; }
			return UpdateStatus.Success;
		}

		private UpdateStatus UpdateAllMaterials(bool @new = false, bool force = false)
		{
			if (@new)
			{
                lock (localVersions)
                {
					localVersions.Remove("allmaterials");
					SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
					File.Delete(ListsDir + MaterialsCompleteJson);
                }
			}

			try { if (!UpdateAvailable(ListType.AllMaterials, force)) return UpdateStatus.Skipped; }
			catch (Exception e)
			{
				Logger.Error(e, "Could not get check for updates when updating all materials. Trying again in an hour should be fine.");
				return UpdateStatus.Fail;
			}

			LoadMappings();

			try
			{
				Parallel.Invoke(
					() => UpdateDevItems(@new, force),
					() => UpdateMaterials(@new, force));

				var data = JToken.Parse(LoadJsonFromFile(MaterialsCompleteJson)).ToObject < Dictionary < string, string > >();
				var dev = JToken.Parse(LoadJsonFromFile(DevMaterialsJson)).ToObject < Dictionary < string, string > >();
				var mats = JToken.Parse(LoadJsonFromFile(MaterialsJson)).ToObject < Dictionary < string, string > >();

				foreach (var item in from item in dev
									 where !data.ContainsKey(item.Key)
									 select item)
				{
					data.Add(item.Key, item.Value);
				}

				foreach (var item in from item in mats
									 where !data.ContainsKey(item.Key)
									 select item)
				{
					data.Add(item.Key, item.Value);
				}

				SaveJson(JsonConvert.SerializeObject(data), MaterialsCompleteJson);
				localVersions["allmaterials"] = RemoteVersion.ToString();
				SaveJson(JsonConvert.SerializeObject(localVersions), versionJson);
			}
			catch (Exception ex) { Logger.Warn(ex); return UpdateStatus.Fail; }
			return UpdateStatus.Success;
		}
		
		public bool UpdateAvailable(ListType? list = null, bool forced = false)
        {
			if (forced) return true;
            try
			{ 
				var lists = new List<string> { "characters", "weapons", "artifacts", "devmaterials", "materials", "allmaterials" };
                foreach (var item in lists)
                {
					if (!File.Exists(ListsDir + item + ".json"))
                    {
						Properties.Settings.Default.LastUpdateCheck = DateTime.MinValue;
						localVersions.Remove(item);
						break;
                    }
                }

				var lastChecked = Properties.Settings.Default.LastUpdateCheck;
				var now = DateTime.Now;

				if (now - lastChecked < TimeSpan.FromHours(1)) return false;

				var remoteVersion = GetRemoteVersion();

				string v;

				if (list.HasValue)
                {
					var localVersion = localVersions.TryGetValue(lists[(int)list], out v) ? new Version(v) : new Version();
					return localVersion.CompareTo(remoteVersion) < 0;
				}
				else
                {
					foreach (var ls in lists)
					{ 
						var localVersion = localVersions.TryGetValue(ls, out v) ? new Version(v) : new Version();
						if (localVersion.CompareTo(remoteVersion) < 0) return true;
					}
					return false;
				}
            }
            catch (Exception e)
            {
				Logger.Error(e, "Could not check for list updates");
				throw;
            }
        }

		private Version GetRemoteVersion()
		{
			var maxCommits = 5;
			using (WebClient client = new WebClient())
			{
				client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				var text = client.DownloadString("https://api.github.com/repos/Dimbreath/GenshinData/commits");
				var response = JArray.Parse(text);
				var commitsChecked = 0;
				foreach (var commit in response.Children())
				{
					if (commitsChecked >= maxCommits) break;

					try
					{
						if (commit["commit"]["message"].ToString().ToUpper().Contains("OSRELWIN"))
						{
							var message = commit["commit"]["message"].ToString();
                            RemoteVersion = new Version(Regex.Match(message, @"[\d\.]*?(?=_)").ToString());
                            if (RemoteVersion != new Version(Properties.Settings.Default.RemoteVersion))
                            {
								Properties.Settings.Default.RemoteVersion = RemoteVersion.ToString();
								Logger.Info("Saved remote version as {0}", Properties.Settings.Default.RemoteVersion);
                            }
                            return RemoteVersion;
						}
					}
					catch (Exception ex) { Logger.Warn(ex); }
					commitsChecked++;
				}
				throw new Exception("Could not determine remote version from commits");
			}
		}

		private string LoadJsonFromURLAsync(string url)
		{
			string json = "";
			using (WebClient client = new WebClient())
			{
				client.Encoding = System.Text.Encoding.UTF8;
				json = client.DownloadString(url);
			}
			return json;
		}

		private string LoadJsonFromFile(string fileName)
		{
            lock (this)
            {
				try
				{
					using (StreamReader file = File.OpenText(ListsDir + fileName))
					using (JsonTextReader reader = new JsonTextReader(file))
					{
						return JToken.ReadFrom(reader).ToString();
					}
				}
				catch (Exception)
				{
					File.Create(ListsDir + fileName).Close();
					return "{}";
				}
            }
		}

		private bool SaveJson(string json, string fileName)
		{
            lock (this)
            {
				try
				{
					using (StreamWriter file = new StreamWriter(ListsDir + fileName))
					using (JsonTextWriter writer = new JsonTextWriter(file))
					{
						writer.Formatting = Formatting.Indented;
						JToken.Parse(json).WriteTo(writer);
					}
					return true;
				}
				catch (Exception ex) { Logger.Warn(ex); return false; }
            }
		}
	}

	public enum ListType
	{
		Characters,
		Weapons,
		Artifacts,
		CharacterDevelopmentItems,
		Materials,
		AllMaterials
	}

	public enum UpdateStatus
    {
		Fail,
		Success,
		Skipped
    }
}
