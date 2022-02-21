using System.Collections.Generic;
using Rage;
using System.Drawing;
using Stealth.Plugins.ALPRPlus.Common;


namespace Stealth.Plugins.ALPRPlus.Core
{
    internal static class BlipHandler
    {
        private static List<Blip> blips = new List<Blip>();

        public static void Attach(Entity entity, Color color)
        {
            var blip = entity.AttachBlip();
            blip.Color = color;
            blip.Alpha = 0.5f;
            blip.Scale = 0.75f;
            blips.Add(blip);
        }

        public static void Clear()
        {
            foreach (var blip in blips)
            {
                try
                {
                    if (blip.Exists())
                    {
                        blip.Delete();
                    }
                }
                catch
                {
                    Logger.LogTrivialDebug("unable to delete blip");
                }
            }

            blips.Clear();
        }
    }
}