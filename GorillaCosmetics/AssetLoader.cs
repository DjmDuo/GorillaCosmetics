﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using GorillaCosmetics.Data;

namespace GorillaCosmetics
{
    public static class AssetLoader
    {
        // there's way too much doubling in this class. make it more modular or something, this hurts to look at.
        static string MaterialsLocation = "Materials";
        static string HatsLocation = "Hats";

        public static bool Loaded = false;
        public static int selectedMaterial = 0; // TODO: better selection with UI
        public static int selectedInfectedMaterial = 0; // TODO: better selection with UI
        public static int selectedHat = 0; // TODO: better selection with UI

        public static IEnumerable<string> MaterialFiles { get; private set; } = Enumerable.Empty<string>();
        public static IList<GorillaMaterial> GorillaMaterialObjects { get; private set; }
        public static IEnumerable<string> HatFiles { get; private set; } = Enumerable.Empty<string>();
        public static IList<GorillaHat> GorillaHatObjects { get; private set; }

        public static GorillaMaterial SelectedMaterial()
        {
            if (!Loaded) return null;
            return GorillaMaterialObjects[selectedMaterial];
        }
        public static GorillaMaterial SelectedInfectedMaterial()
        {
            if (!Loaded) return null;
            return GorillaMaterialObjects[selectedInfectedMaterial];
        }
        public static GorillaHat SelectedHat()
        {
            if (!Loaded) return null;
            return GorillaHatObjects[selectedHat];
        }

        public static void Load()
        {
            if (Loaded) return;
            string folder = Path.GetDirectoryName(typeof(GorillaCosmetics).Assembly.Location);

            // materials
            IEnumerable<string> filter = new List<string> { "*.material", "*.gmat" };
            MaterialFiles = GetFileNames($"{folder}\\{MaterialsLocation}", filter, SearchOption.TopDirectoryOnly, false);
            GorillaMaterialObjects = LoadMaterials(MaterialFiles);

            // hats
            IEnumerable<string> hatFilter = new List<string> { "*.hat", "*.ghat" };
            HatFiles = GetFileNames($"{folder}\\{HatsLocation}", hatFilter, SearchOption.TopDirectoryOnly, false);
            GorillaHatObjects = LoadHats(HatFiles);

            //config parsing
            selectedMaterial = SelectedMaterialFromConfig(GorillaCosmetics.selectedMaterial.Value);
            selectedInfectedMaterial = SelectedMaterialFromConfig(GorillaCosmetics.selectedInfectedMaterial.Value);
            selectedHat = SelectedHatFromConfig();

            Loaded = true;
        }

        public static int SelectedMaterialFromConfig(string configString)
        {
            string selectedMatString = configString.ToLower().Trim();
            for (int i = 1; i < GorillaMaterialObjects.Count; i++)
            {
                GorillaMaterial gorillaMaterialObject = GorillaMaterialObjects[i];
                if (gorillaMaterialObject == null) return 0;
                if (gorillaMaterialObject.Descriptor.MaterialName.ToLower().Trim() == selectedMatString)
                {
                    return i;
                }
                else if (Path.GetFileNameWithoutExtension(gorillaMaterialObject.FileName).ToLower().Trim() == selectedMatString)
                {
                    return i;
                }
            }
            return 0;
        }

        public static int SelectedHatFromConfig()
        {
            string selectedHatString = GorillaCosmetics.selectedHat.Value.ToLower().Trim();
            for (int i = 1; i < GorillaHatObjects.Count; i++)
            {
                GorillaHat gorillaHatObject = GorillaHatObjects[i];
                if (gorillaHatObject == null) return 0;
                if (gorillaHatObject.Descriptor.HatName.ToLower().Trim() == selectedHatString)
                {
                    return i;
                }
                else if (Path.GetFileNameWithoutExtension(gorillaHatObject.FileName).ToLower().Trim() == selectedHatString)
                {
                    return i;
                }
            }
            return 0;
        }

        public static IList<GorillaMaterial> LoadMaterials(IEnumerable<string> materialFiles)
        {
            IList<GorillaMaterial> materials = new List<GorillaMaterial> { new GorillaMaterial("Default") };
            foreach (string materialFile in materialFiles)
            {
                try
                {
                    GorillaMaterial material = new GorillaMaterial(materialFile);
                    materials.Add(material);
                }
                catch (Exception ex)
                {
                    Debug.Log("ERROR!");
                    Debug.Log(ex);
                }
            }
            return materials;
        }

        public static IList<GorillaHat> LoadHats(IEnumerable<string> hatFiles)
        {
            IList<GorillaHat> hats = new List<GorillaHat> { new GorillaHat("Default") };
            foreach (string hatFile in hatFiles)
            {
                try
                {
                    GorillaHat hat = new GorillaHat(hatFile);
                    hats.Add(hat);
                }
                catch (Exception ex)
                {
                    Debug.Log("ERROR!");
                    Debug.Log(ex);
                }
            }
            return hats;
        }

        /// <summary>
        /// Gets every file matching the filter in a path.
        /// </summary>
        /// <param name="path">Directory to search in.</param>
        /// <param name="filters">Pattern(s) to search for.</param>
        /// <param name="searchOption">Search options.</param>
        /// <param name="returnShortPath">Remove path from filepaths.</param>
        public static IEnumerable<string> GetFileNames(string path, IEnumerable<string> filters, SearchOption searchOption, bool returnShortPath = false)
        {
            IList<string> filePaths = new List<string>();

            foreach (string filter in filters)
            {
                IEnumerable<string> directoryFiles = Directory.GetFiles(path, filter, searchOption);

                if (returnShortPath)
                {
                    foreach (string directoryFile in directoryFiles)
                    {
                        string filePath = directoryFile.Replace(path, "");
                        if (filePath.Length > 0 && filePath.StartsWith(@"\"))
                        {
                            filePath = filePath.Substring(1, filePath.Length - 1);
                        }

                        if (!string.IsNullOrWhiteSpace(filePath) && !filePaths.Contains(filePath))
                        {
                            filePaths.Add(filePath);
                        }
                    }
                }
                else
                {
                    filePaths = filePaths.Union(directoryFiles).ToList();
                }
            }

            return filePaths.Distinct();
        }
    }
}