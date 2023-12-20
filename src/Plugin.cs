using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace LCPosterEditor
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static List<string> PosterFolders = new List<string>();
        public static readonly List<string> PosterFiles = new List<string>();
        public static readonly List<string> TipFiles = new List<string>();
        public static Random Rand = new Random();

        public static ConfigEntry<bool> KeepAspectRatio;

        private void Awake()
        {
            LoadConfigs();

            CreateDirs();
            if ((!Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "posters")).Any() &&
                Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "posters")).Any()) ||
                (!Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "tips")).Any() &&
                (Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "tips")).Any())))
            {
                AtlasCreator.Init(Logger);
            }
            else
            {
                Logger.LogInfo("Poster/Tip files found or Input files missing... continuing to load");
            }

            PosterFolders = Directory.GetDirectories(Paths.PluginPath, PluginInfo.PLUGIN_NAME, SearchOption.AllDirectories).ToList();
            PosterFolders.AddRange(Directory.GetDirectories(Paths.PluginPath, "LethalPosters", SearchOption.AllDirectories).ToList());
            
            foreach (var folder in PosterFolders)
            {
                foreach (var file in Directory.GetFiles(Path.Combine(folder, "posters")))
                {
                    if (Path.GetExtension(file) != ".old")
                    {
                        PosterFiles.Add(file);
                    }
                }

                foreach (var file in Directory.GetFiles(Path.Combine(folder, "tips")))
                {
                    if (Path.GetExtension(file) != ".old")
                    {
                        TipFiles.Add(file);
                    }
                }
            }

            Patches.Init(Logger);

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
            
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} ({PluginInfo.PLUGIN_VERSION}) is loaded!");
        }

        private void CreateDirs()
        {
            Directory.CreateDirectory(Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "posters"));
            Directory.CreateDirectory(Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "tips"));
            Directory.CreateDirectory(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "posters"));
            Directory.CreateDirectory(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "tips"));
        }

        private void LoadConfigs()
        {
            KeepAspectRatio = ((BaseUnityPlugin)this).Config.Bind<bool>("Settings", "KeepAspectRatio", true, "Determines whether or not to restrict the image generation to the original image's aspect ratio");
        }
    }
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "nyoo.LCPosterEditor";
        public const string PLUGIN_NAME = "LCPosterEditor";
        public const string PLUGIN_VERSION = "1.0.0";
        public static readonly string PLUGIN_DIR = Path.Combine(Paths.PluginPath, PluginInfo.PLUGIN_GUID);
    }

}
