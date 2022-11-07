using Rage;
using Stealth.Plugins.ALPRPlus.API.Types.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Stealth.Plugins.ALPRPlus.Common
{
    internal static class Config
    {
        private const string mFileName = "ALPRPlus.ini";
        private const string mFilePath = @"Plugins\LSPDFR\" + mFileName;
        private static InitializationFile mCfgFile = new InitializationFile(mFilePath);

        //internal static string BetaKey { get; set; }

        internal static Keys ToggleKey { get; set; }
        internal static Keys ToggleKeyModifier { get; set; }
        internal static bool PlayAlertSound { get; set; } = true;
        internal static bool PlayScanSound { get; set; } = true;
        internal static bool AutoDisableOnTrafficStops { get; set; } = true;
        internal static bool AutoDisableOnPursuits { get; set; } = true;

        internal static Keys MenuKey { get; set; }
        internal static Keys MenuKeyModifier { get; set; }
        internal static bool EnableDriverFrontCam { get; set; } = true;
        internal static bool EnableDriverRearCam { get; set; } = true;
        internal static bool EnablePassengerFrontCam { get; set; } = true;
        internal static bool EnablePassengerRearCam { get; set; } = true;
        internal static bool EnableMobileANPRCam { get; set; } = true;
        internal static bool EnableMobileANPRCam2 { get; set; } = true;


        private const int cDriverFrontAngle = 150;
        private const int cDriverRearAngle = 210;
        private const int cPassengerRearAngle = 330;
        private const int cPassengerFrontAngle = 30;
        private const int cMobileANPRAngle = 330;
        private const int cMobileANPRAngle2 = 150;

        private const int cCameraDegreesFOV = 50;
        private const float cCameraRange = 20f;
        private const float cCameraMinRange = 2f;

        internal static int CameraDegreesFOV { get; set; } = cCameraDegreesFOV;
        internal static float CameraRange { get; set; } = cCameraRange;
        internal static float CameraMinimum { get; set; } = cCameraMinRange;
        internal static int DriverFrontAngle { get; set; } = cDriverFrontAngle;
        internal static int DriverRearAngle { get; set; } = cDriverRearAngle;
        internal static int PassengerRearAngle { get; set; } = cPassengerRearAngle;
        internal static int PassengerFrontAngle { get; set; } = cPassengerFrontAngle;
        internal static int MobileANPRAngle { get; set; } = cMobileANPRAngle;
        internal static int MobileANPRAngle2 { get; set; } = cMobileANPRAngle2;
        internal static int VehicleRescanBuffer { get; set; } = cVehicleRescanBuffer;
        internal static int VehicleRescanBufferInMilliseconds { get { return VehicleRescanBuffer * 1000; } }

        private const int cSecondsBetweenAlerts = 120;
        private const int cProbabilityOfAlerts = 8;
        private const int cDefaultStolenVehicleWeight = 10;
        private const int cDefaultUnregisteredVehicleWeight = 10;
        private const int cDefaultRegistrationExpiredWeight = 15;
        private const int cDefaultNoInsuranceWeight = 10;
        private const int cDefaultInsuranceExpiredWeight = 15;
        private const int cDefaultUnTaxedWeight = 15;
        private const int cDefaultDrugsMarkerWeight = 15;
        private const int cVehicleRescanBuffer = 30;

        internal static int SecondsBetweenAlerts { get; set; } = cSecondsBetweenAlerts;
        internal static int ProbabilityOfAlerts { get; set; } = cProbabilityOfAlerts;
        internal static int StolenVehicleWeight { get; set; } = cDefaultStolenVehicleWeight;
        internal static int UnregisteredVehicleWeight { get; set; } = cDefaultUnregisteredVehicleWeight;
        internal static int RegistrationExpiredWeight { get; set; } = cDefaultRegistrationExpiredWeight;
        internal static int NoInsuranceWeight { get; set; } = cDefaultNoInsuranceWeight;
        internal static int InsuranceExpiredWeight { get; set; } = cDefaultInsuranceExpiredWeight;
        internal static int UnTaxedWeight { get; set; } = cDefaultUnTaxedWeight;
        internal static int DrugsMarkerWeight { get; set; } = cDefaultDrugsMarkerWeight;

        internal static void Init()
        {
            if (!mCfgFile.Exists())
            {
                Logger.LogTrivial("Config file does not exist; creating...");
                CreateCfg();
            }

            ReadCfg();
        }

        private static void CreateCfg()
        {
            mCfgFile.Create();

            Logger.LogTrivial("Filling config file with default settings...");
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.ToggleKey.ToString(), Keys.F8.ToString());
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.ToggleKeyModifier.ToString(), Keys.None.ToString());
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.MenuKey.ToString(), Keys.Y.ToString());
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.MenuKeyModifier.ToString(), Keys.None.ToString());
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.PlayAlertSound.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.PlayScanSound.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.AutoDisableOnTrafficStops.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.AutoDisableOnPursuits.ToString(), true.ToString());
            //mCfgFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.BetaKey.ToString(), "YourBetaKeyHere");

            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.CameraDegreesFOV.ToString(), cCameraDegreesFOV);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.CameraRange.ToString(), cCameraRange);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.CameraMinimum.ToString(), cCameraMinRange);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.DriverFrontAngle.ToString(), cDriverFrontAngle);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.DriverRearAngle.ToString(), cDriverRearAngle);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.PassengerRearAngle.ToString(), cPassengerRearAngle);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.PassengerFrontAngle.ToString(), cPassengerFrontAngle);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.MobileANPRAngle.ToString(), cMobileANPRAngle);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.MobileANPRAngle2.ToString(), cMobileANPRAngle2);
            mCfgFile.Write(ECfgSections.CAMERAS.ToString(), ECameras.VehicleRescanBuffer.ToString(), cVehicleRescanBuffer);

            mCfgFile.Write(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableDriverFrontCam.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableDriverRearCam.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnablePassengerFrontCam.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnablePassengerRearCam.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableMobileANPRCam.ToString(), true.ToString());
            mCfgFile.Write(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableMobileANPRCam2.ToString(), true.ToString());

            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.SecondsBetweenAlerts.ToString(), cSecondsBetweenAlerts);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.ProbabilityOfAlerts.ToString(), cProbabilityOfAlerts);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.StolenVehicleWeight.ToString(), cDefaultStolenVehicleWeight);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.UnregisteredVehicleWeight.ToString(), cDefaultUnregisteredVehicleWeight);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.RegistrationExpiredWeight.ToString(), cDefaultRegistrationExpiredWeight);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.NoInsuranceWeight.ToString(), cDefaultNoInsuranceWeight);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.InsuranceExpiredWeight.ToString(), cDefaultInsuranceExpiredWeight);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.UnTaxedWeight.ToString(), cDefaultUnTaxedWeight);
            mCfgFile.Write(ECfgSections.ALERTS.ToString(), EAlerts.DrugsMarkerWeight.ToString(), cDefaultDrugsMarkerWeight);
        }

        private static void ReadCfg()
        {
            Logger.LogTrivial("Reading settings from config file...");

            ToggleKey = mCfgFile.ReadEnum<Keys>(ECfgSections.SETTINGS.ToString(), ESettings.ToggleKey.ToString(), Keys.F8);
            ToggleKeyModifier = mCfgFile.ReadEnum<Keys>(ECfgSections.SETTINGS.ToString(), ESettings.ToggleKeyModifier.ToString(), Keys.None);
            MenuKey = mCfgFile.ReadEnum<Keys>(ECfgSections.SETTINGS.ToString(), ESettings.MenuKey.ToString(), Keys.Y);
            MenuKeyModifier = mCfgFile.ReadEnum<Keys>(ECfgSections.SETTINGS.ToString(), ESettings.MenuKeyModifier.ToString(), Keys.None);
            PlayAlertSound = mCfgFile.ReadBoolean(ECfgSections.SETTINGS.ToString(), ESettings.PlayAlertSound.ToString(), true);
            PlayScanSound = mCfgFile.ReadBoolean(ECfgSections.SETTINGS.ToString(), ESettings.PlayScanSound.ToString(), true);
            AutoDisableOnTrafficStops = mCfgFile.ReadBoolean(ECfgSections.SETTINGS.ToString(), ESettings.AutoDisableOnTrafficStops.ToString(), true);
            AutoDisableOnPursuits = mCfgFile.ReadBoolean(ECfgSections.SETTINGS.ToString(), ESettings.AutoDisableOnPursuits.ToString(), true);

            EnableDriverFrontCam = mCfgFile.ReadBoolean(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableDriverFrontCam.ToString(), true);
            EnableDriverRearCam = mCfgFile.ReadBoolean(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableDriverRearCam.ToString(), true);
            EnablePassengerFrontCam = mCfgFile.ReadBoolean(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnablePassengerFrontCam.ToString(), true);
            EnablePassengerRearCam = mCfgFile.ReadBoolean(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnablePassengerRearCam.ToString(), true);
            EnableMobileANPRCam = mCfgFile.ReadBoolean(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableMobileANPRCam.ToString(), true);
            EnableMobileANPRCam2 = mCfgFile.ReadBoolean(ECfgSections.CAMERATOGGLE.ToString(), ECameratoggle.EnableMobileANPRCam2.ToString(), true);
            //BetaKey = mCfgFile.ReadString(ECfgSections.SETTINGS.ToString(), ESettings.BetaKey.ToString(), "YourBetaKeyHere");

            Logger.LogTrivial("ToggleKey = " + ToggleKey.ToString());

            CameraDegreesFOV = mCfgFile.ReadInt32(ECfgSections.CAMERAS.ToString(), ECameras.CameraDegreesFOV.ToString(), cCameraDegreesFOV);
            CameraRange = (float)mCfgFile.ReadDouble(ECfgSections.CAMERAS.ToString(), ECameras.CameraRange.ToString(), cCameraRange);
            CameraMinimum = (float)mCfgFile.ReadDouble(ECfgSections.CAMERAS.ToString(), ECameras.CameraMinimum.ToString(), (float)cCameraMinRange);
            DriverFrontAngle = mCfgFile.ReadInt32(ECfgSections.CAMERAS.ToString(), ECameras.DriverFrontAngle.ToString(), cDriverFrontAngle);
            DriverRearAngle = mCfgFile.ReadInt32(ECfgSections.CAMERAS.ToString(), ECameras.DriverRearAngle.ToString(), cDriverRearAngle);
            PassengerRearAngle = mCfgFile.ReadInt32(ECfgSections.CAMERAS.ToString(), ECameras.PassengerRearAngle.ToString(), cPassengerRearAngle);
            PassengerFrontAngle = mCfgFile.ReadInt32(ECfgSections.CAMERAS.ToString(), ECameras.PassengerFrontAngle.ToString(), cPassengerFrontAngle);
            MobileANPRAngle = mCfgFile.ReadInt32(ECfgSections.CAMERAS.ToString(), ECameras.MobileANPRAngle.ToString(), cMobileANPRAngle);
            VehicleRescanBuffer = mCfgFile.ReadInt32(ECfgSections.CAMERAS.ToString(), ECameras.VehicleRescanBuffer.ToString(), cVehicleRescanBuffer);

            SecondsBetweenAlerts = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.SecondsBetweenAlerts.ToString(), cSecondsBetweenAlerts);
            ProbabilityOfAlerts = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.ProbabilityOfAlerts.ToString(), cProbabilityOfAlerts);
            StolenVehicleWeight = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.StolenVehicleWeight.ToString(), cDefaultStolenVehicleWeight);
            UnregisteredVehicleWeight = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.UnregisteredVehicleWeight.ToString(), cDefaultUnregisteredVehicleWeight);
            RegistrationExpiredWeight = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.RegistrationExpiredWeight.ToString(), cDefaultRegistrationExpiredWeight);
            NoInsuranceWeight = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.NoInsuranceWeight.ToString(), cDefaultNoInsuranceWeight);
            InsuranceExpiredWeight = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.InsuranceExpiredWeight.ToString(), cDefaultInsuranceExpiredWeight);
            UnTaxedWeight = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.UnTaxedWeight.ToString(), cDefaultUnTaxedWeight);
            DrugsMarkerWeight = mCfgFile.ReadInt32(ECfgSections.ALERTS.ToString(), EAlerts.DrugsMarkerWeight.ToString(), cDefaultDrugsMarkerWeight);
            AdjustAlertWeights();
        }

        public static void SaveINI()
        {
            InitializationFile ini = new InitializationFile($"plugins/LSPDFR/ALPRPlus.ini");
            if (ini.Exists())
                ini.Delete();
            ini.Create();
            ini.Write("Settings", "ToggleKey", ToggleKey.ToString());
            ini.Write("Settings", "ToggleKeyModifier", ToggleKeyModifier.ToString());
            ini.Write("Settings", "MenuKey", MenuKey.ToString());
            ini.Write("Settings", "MenuKeyModifier", MenuKeyModifier.ToString());
            ini.Write("Settings", "PlayAlertSound", PlayAlertSound);
            ini.Write("Settings", "PlayScanSound", PlayScanSound);
            ini.Write("Settings", "AutoDisableOnTrafficStops", AutoDisableOnTrafficStops);
            ini.Write("Settings", "AutoDisableOnPursuits", AutoDisableOnPursuits);

            ini.Write("CameraToggle", "EnableDriverFrontCam", EnableDriverFrontCam);
            ini.Write("CameraToggle", "EnableDriverRearCam", EnableDriverRearCam);
            ini.Write("CameraToggle", "EnablePassengerFrontCam", EnablePassengerFrontCam);
            ini.Write("CameraToggle", "EnablePassengerRearCam", EnablePassengerRearCam);
            ini.Write("CameraToggle", "EnableMobileANPRCam", EnableMobileANPRCam);
            ini.Write("CameraToggle", "EnableMobileANPRCam2", EnableMobileANPRCam2);

            ini.Write("Cameras", "CameraDegreesFOV", CameraDegreesFOV);
            ini.Write("Cameras", "CameraRange", CameraRange);
            ini.Write("Cameras", "CameraMinimum", CameraMinimum);
            ini.Write("Cameras", "DriverFrontAngle", DriverFrontAngle);
            ini.Write("Cameras", "DriverRearAngle", DriverRearAngle);
            ini.Write("Cameras", "PassengerRearAngle", PassengerRearAngle);
            ini.Write("Cameras", "PassengerFrontAngle", PassengerFrontAngle);
            ini.Write("Cameras", "MobileANPRAngle", MobileANPRAngle);
            ini.Write("Cameras", "MobileANPRAngle2", MobileANPRAngle2);
            ini.Write("Cameras", "VehicleRescanBuffer", VehicleRescanBuffer);

            ini.Write("Alerts", "SecondsBetweenAlerts", SecondsBetweenAlerts);
            ini.Write("Alerts", "ProbabilityOfAlerts", ProbabilityOfAlerts);
            ini.Write("Alerts", "StolenVehicleWeight", StolenVehicleWeight);
            ini.Write("Alerts", "UnregisteredVehicleWeight", UnregisteredVehicleWeight);
            ini.Write("Alerts", "RegistrationExpiredWeight", RegistrationExpiredWeight);
            ini.Write("Alerts", "NoInsuranceWeight", NoInsuranceWeight);
            ini.Write("Alerts", "InsuranceExpiredWeight", InsuranceExpiredWeight);
            ini.Write("Alerts", "UnTaxedWeight", UnTaxedWeight);
            ini.Write("Alerts", "DrugsMarkerWeight", DrugsMarkerWeight);
            Game.DisplaySubtitle("Saved configuration");
        }
            private static void AdjustAlertWeights()
        {
            Dictionary<EAlertType, int> mAlertWeights = new Dictionary<EAlertType, int>() {
                {EAlertType.Stolen_Vehicle, Config.StolenVehicleWeight },
                {EAlertType.Unregistered_Vehicle, Config.UnregisteredVehicleWeight},
                {EAlertType.Registration_Expired, Config.RegistrationExpiredWeight},
                {EAlertType.No_Insurance, Config.NoInsuranceWeight},
                {EAlertType.Insurance_Expired, Config.InsuranceExpiredWeight},
                {EAlertType.UnTaxed, Config.UnTaxedWeight},
                {EAlertType.DrugsMarker, Config.DrugsMarkerWeight}
            };

            int mTotal = (from x in mAlertWeights select x.Value).Sum();

            if (mTotal != 100)
            {
                List<EAlertType> keys = (from x in mAlertWeights select x.Key).ToList();

                foreach (var x in keys)
                {
                    double mAdjustedWeight = (mAlertWeights[x] / 100);
                    int mActualWeight = Convert.ToInt32(Math.Floor(mAdjustedWeight));
                    mAlertWeights[x] = mActualWeight;
                }

                Config.StolenVehicleWeight = mAlertWeights[EAlertType.Stolen_Vehicle];
                Config.UnregisteredVehicleWeight = mAlertWeights[EAlertType.Unregistered_Vehicle];
                Config.RegistrationExpiredWeight = mAlertWeights[EAlertType.Registration_Expired];
                Config.NoInsuranceWeight = mAlertWeights[EAlertType.No_Insurance];
                Config.InsuranceExpiredWeight = mAlertWeights[EAlertType.Insurance_Expired];
                Config.UnTaxedWeight = mAlertWeights[EAlertType.UnTaxed];
                Config.DrugsMarkerWeight = mAlertWeights[EAlertType.DrugsMarker];
            }
        }

        internal static string GetToggleKeyString()
        {
            return GetKeyString(ToggleKey, ToggleKeyModifier);
        }

        private static string GetKeyString(Keys key, Keys modKey)
        {
            if (modKey == Keys.None)
            {
                return key.ToString();
            }
            else
            {
                string strmodKey = modKey.ToString();

                if (strmodKey.EndsWith("ControlKey") | strmodKey.EndsWith("ShiftKey"))
                {
                    strmodKey.Replace("Key", "");
                }

                if (strmodKey.Contains("ControlKey"))
                {
                    strmodKey = "CTRL";
                }
                else if (strmodKey.Contains("ShiftKey"))
                {
                    strmodKey = "Shift";
                }
                else if (strmodKey.Contains("Menu"))
                {
                    strmodKey = "ALT";
                }

                return string.Format("{0} + {1}", strmodKey, key.ToString());
            }
        }

        private enum ECfgSections
        {
            SETTINGS, CAMERAS, CAMERATOGGLE, ALERTS
        }

        private enum ESettings
        {
            ToggleKey, ToggleKeyModifier,MenuKey, MenuKeyModifier, PlayAlertSound, PlayScanSound, AutoDisableOnTrafficStops, AutoDisableOnPursuits, BetaKey
        }

        private enum ECameratoggle
        {
            EnableDriverFrontCam, EnableDriverRearCam, EnablePassengerFrontCam, EnablePassengerRearCam, EnableMobileANPRCam, EnableMobileANPRCam2
        }


        private enum ECameras
        {
            CameraDegreesFOV,
            CameraRange,
            CameraMinimum,
            DriverFrontAngle,
            DriverRearAngle,
            PassengerRearAngle,
            PassengerFrontAngle,
            MobileANPRAngle,
            MobileANPRAngle2,
            VehicleRescanBuffer
        }

        private enum EAlerts
        {
            SecondsBetweenAlerts,
            ProbabilityOfAlerts,
            StolenVehicleWeight,
            OwnerWantedWeight,
            UnregisteredVehicleWeight,
            RegistrationExpiredWeight,
            NoInsuranceWeight,
            InsuranceExpiredWeight,
            UnTaxedWeight,
            DrugsMarkerWeight
        }
    }
}
