﻿using System;
using System.Linq;


namespace Spotlight_Desktop
{
    internal static class FindImage
    {
        private const string CreativePath =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\Creative";
        private const string OldCreativePath =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen\Creative";
        private static readonly string UserLocalAppData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);


        // This is done in a specifc order
        // ...\LogonUI\Creative\[Phase 1]\[Phase 2]
        public static string FindCurrentImage()
        {
            // Check the Creative subkeys
            var creative = new RegLookupParse(CreativePath);

            // *Currently used on the latest Windows 10 Release*
            // This goes through [Phase 1] SubKeys, then the last [Phase 2] SubKey
            foreach (var currentPhase1Key in creative.SubKeys)
            {
                if (currentPhase1Key.SubKeyObj.SubKeys.Count != 0)
                {
                    foreach (var phase2KeyValue in currentPhase1Key.SubKeyObj.SubKeys.Last().SubKeyObj.KeyValues)
                    {
                        if (phase2KeyValue.Name == "landscapeImage")
                        {
                            // Check to make sure it belongs to this account
                            if (phase2KeyValue.Value.StartsWith(UserLocalAppData))
                            {
                                return phase2KeyValue.Value;
                            }

                            break;
                        }
                    }
                }
            }

            // This goes through [Phase 1] SubKeys, and searches it's Key Values for "LandscapeAssetPath"
            // (Used for an older Windows 10 update where [Phase 2] didn't exsist)
            // (This does not go into [Phase 2])
            foreach (var currentPhase1Key in creative.SubKeys)
            {
                // We have a [Phase 2] SubKey, now find the "landscapeImage" in the KeyValues
                foreach (var phase2KeyValue in currentPhase1Key.SubKeyObj.KeyValues)
                {
                    if (phase2KeyValue.Name == "LandscapeAssetPath")
                    {
                        // Check to make sure it belongs to this account
                        if (phase2KeyValue.Value.StartsWith(UserLocalAppData))
                        {
                            return phase2KeyValue.Value;
                        }

                        break;
                    }
                }
            }

            // Should occur if on an old Windows Update
            var oldCreative = new RegLookupParse(OldCreativePath, false);

            if (!oldCreative.Error)
            {
                foreach (var keyName in oldCreative.KeyValues)
                {
                    if (keyName.Name == "LandscapeAssetPath")
                    {
                        if (keyName.Value.StartsWith(UserLocalAppData))
                        {
                            return keyName.Value;
                        }
                    }
                }
            }

            return null;
        }
    }
}
