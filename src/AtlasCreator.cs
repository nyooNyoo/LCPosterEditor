using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using UnityEngine;

namespace LCPosterEditor
{
    internal class AtlasCreator
    {
        static ManualLogSource Logger {get; set;}

        private static List<Image> posterImages = Directory.EnumerateFiles(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "posters")).Select(f => Image.FromFile(f)).ToList();
        private static List<Image> tipImages = Directory.EnumerateFiles(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "tips")).Select(f => Image.FromFile(f)).ToList();

        private static readonly int[] TIPS_SIZE = {796, 1024};
        private static readonly int[] POSTER_SIZE = {1024, 1024};
        private static readonly int[,] POSTER_OFFSETS = {{0, 0, 341, 559},
                                                  {346, 0, 284, 559},
                                                  {641, 58, 274, 243},
                                                  {184, 620, 411, 364},
                                                  {632, 320, 372, 672}};

        public static void Init(ManualLogSource logger)
        {
            Logger = logger;
            Logger.LogInfo("Poster and/or Tips images found, creating assets now...");
            if (!Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "posters")).Any() &&
                Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "posters")).Any())
            {
                Logger.LogInfo("Creating Poster Atlases...");
                for (int i = 0; i < posterImages.Count; i++)
                {
                    List<Image> posterGroup = new List<Image>();
                    for (int j = 0; j < 5; j++)
                    {
                        posterGroup.Add(ResizeImage(posterImages[(i + j) % posterImages.Count], POSTER_OFFSETS[j, 2], POSTER_OFFSETS[j, 3], Plugin.KeepAspectRatio.Value));
                    }
                    CreatePosterAtlas(posterGroup, Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "posters", $"{i}.png"));
                }
                Logger.LogInfo("Poster Atlases Created!");
            }
            if (!Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "tips")).Any() &&
                Directory.EnumerateFileSystemEntries(Path.Combine(PluginInfo.PLUGIN_DIR, "Input", "tips")).Any())
            {
                Logger.LogInfo("Creating Tip Files...");
                for (int i = 0; i < tipImages.Count; i++)
                {
                    CreateTipFile(ResizeImage(tipImages[i], TIPS_SIZE[0], TIPS_SIZE[1], Plugin.KeepAspectRatio.Value), Path.Combine(PluginInfo.PLUGIN_DIR, PluginInfo.PLUGIN_NAME, "tips", $"{i}.png"));
                }
                Logger.LogInfo("Tip Files Created!");
            }
        }

        private static void CreatePosterAtlas(List<Image> posters, string filename)
        {
            Image atlas = new Bitmap(POSTER_SIZE[0], POSTER_SIZE[1]);
            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(atlas))
            {
                for (int i = 0; i < posters.Count; i++)
                {
                    gr.DrawImage(posters[i], new Point(POSTER_OFFSETS[i, 0], POSTER_OFFSETS[i, 1]));
                }
            }
            atlas.Save(filename);
        }

        private static void CreateTipFile(Image tip, string filename)
        {
            tip.Save(filename);
        }

        private static Bitmap ResizeImage(Image image, int width, int height, bool keepAspectRatio)
        {
            if (keepAspectRatio)
            {
                if (image.Width > image.Height)
                {
                    height = height * width/image.Width;
                }
                else
                {
                    width = width * height/image.Height;
                }
            }

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}
