﻿using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Rage;
using StopThePed;
using Stealth.Common.Extensions;
using Stealth.Common.Models;
using Stealth.Plugins.ALPRPlus.API.Types;
using Stealth.Plugins.ALPRPlus.API.Types.Enums;
using Stealth.Plugins.ALPRPlus.Common;
using Stealth.Plugins.ALPRPlus.Common.Enums;
using Stealth.Plugins.ALPRPlus.Extensions;
using Stealth.Plugins.ALPRPlus.Mods;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stealth.Plugins.ALPRPlus.Core
{
    internal class ALPR
    {
        private Dictionary<Vehicle, uint> mRecentlyScanned = new Dictionary<Vehicle, uint>();
        private Random mRand = null;
        private static readonly Random rand = new Random();


        internal ALPR()
        {
            mRand = new Random(Guid.NewGuid().GetHashCode());
        }

        internal void ScanPlates()
        {
            if (!Globals.PlayerPed.IsOnFoot && Globals.PlayerVehicle.Exists())
            {
                if (Globals.PlayerVehicle.IsPoliceVehicle && IsActive(Globals.PlayerVehicle))
                {
                    List<Vehicle> mVehicles = World.EnumerateVehicles().ToList();
                    mVehicles = (from x in mVehicles
                                 where x.Exists() && !IsVehicleBlacklisted(x) &&
             x.DistanceTo(Globals.PlayerPed.Position) <= Config.CameraRange &&
             x.DistanceTo(Globals.PlayerPed.Position) >= Config.CameraMinimum
                                 orderby x.DistanceTo(Globals.PlayerPed.Position)
                                 select x).ToList();

                    foreach (Vehicle veh in mVehicles)
                    {
                        if (Globals.AlertTimer.IsRunning)
                        {
                            if (Globals.AlertTimer.Elapsed.TotalSeconds < Config.SecondsBetweenAlerts)
                            {
                                break;
                            }
                            else
                            {
                                Globals.AlertTimer.Stop();
                                Globals.AlertTimer.Reset();
                            }
                        }

                        if (veh.Exists())
                        {
                            ECamera cam = GetCapturingCamera(veh);

                            if (cam != ECamera.Null)
                            {
                                //Logger.LogTrivialDebug("Camera -- " + cam.ToFriendlyString());

                                bool mAlertTriggered = RunVehiclePlates(veh, cam);

                                if (mAlertTriggered && !Globals.AlertTimer.IsRunning)
                                {
                                    Globals.AlertTimer.Start();
                                }
                            }
                        }

                        GameFiber.Sleep(250);
                    }

                    List<Vehicle> vehToRemove = mRecentlyScanned.Where(x => Game.GameTime > (x.Value + Config.VehicleRescanBufferInMilliseconds)).Select(x => x.Key).ToList();

                    foreach (Vehicle v in vehToRemove)
                    {
                        mRecentlyScanned.Remove(v);
                    }
                }
            }
        }

        private bool RunVehiclePlates(Vehicle veh, ECamera cam)
        {
            if (veh.Exists() && !IsVehicleBlacklisted(veh))
            {
                if (!mRecentlyScanned.ContainsKey(veh))
                {
                    mRecentlyScanned.Add(veh, Game.GameTime);

                    if (Config.PlayScanSound)
                    {
                        Audio.PlaySound(Audio.ESounds.PinButton);
                        //GameFiber.Sleep(500);
                    }

                    if (Globals.ScanResults.Keys.Contains(veh))
                    {
                        TimeSpan ts = DateTime.Now - Globals.ScanResults[veh].LastDisplayed;
                        if (ts.TotalSeconds < Config.VehicleRescanBuffer)
                        {
                            return false;
                        }

                        EAlertType mPrevAlert = Globals.ScanResults[veh].AlertType;

                        if (mPrevAlert != EAlertType.Null)
                        {
                            DisplayAlert(veh, cam, Globals.ScanResults[veh]);
                            return true;
                        }
                        else
                        {
                            if (Globals.ScanResults[veh].IsCustomFlag == true && Globals.ScanResults[veh].Result != "")
                            {
                                DisplayAlert(veh, cam, Globals.ScanResults[veh]);
                                return true;
                            }
                        }

                        return false;
                    }
                    else
                    {
                        int mAlertFactor = mRand.Next(0, 100);

                        if (mAlertFactor < Config.ProbabilityOfAlerts)
                        {
                            EAlertType mGeneratedFlag = GenerateFlag(veh);

                            if (mGeneratedFlag != EAlertType.Null)
                            {
                                API.ALPRScanResult r = CreateScanResult(veh, mGeneratedFlag, cam);
                                DisplayAlert(veh, cam, r);
                                return true;
                            }
                            else
                            {
                                if (!Globals.ScanResults.ContainsKey(veh))
                                {
                                    API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.Null);
                                    Globals.ScanResults.Add(veh, r);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (!Globals.ScanResults.ContainsKey(veh))
                            {
                                API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.Null);
                                Globals.ScanResults.Add(veh, r);
                                return false;
                            }
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        private EAlertType CheckForExistingFlags(Vehicle veh)
        {
            if (veh.Exists() && (veh.HasDriver && veh.Driver.Exists()))
            {

                if (veh.IsStolen)
                {
                    return EAlertType.Stolen_Vehicle;
                }
                if (Funcs.IsSTPRunning())
                {
                    StopThePed.API.STPVehicleStatus mRegistration = StopThePed.API.Functions.getVehicleRegistrationStatus(veh);
                    StopThePed.API.STPVehicleStatus mInsurance = StopThePed.API.Functions.getVehicleInsuranceStatus(veh);

                    if (mRegistration == StopThePed.API.STPVehicleStatus.Expired)
                        return EAlertType.Registration_Expired;
                    else if (mRegistration == StopThePed.API.STPVehicleStatus.None)
                        return EAlertType.Unregistered_Vehicle;

                    if (mInsurance == StopThePed.API.STPVehicleStatus.Expired)
                        return EAlertType.Insurance_Expired;
                    else if (mInsurance == StopThePed.API.STPVehicleStatus.None)
                        return EAlertType.No_Insurance;
                }
            }
            else if (veh.Exists() && (!veh.HasDriver || !veh.Driver.Exists()))
            {
                if (veh.IsStolen)
                {
                    return EAlertType.Stolen_Vehicle;
                }

                if (Funcs.IsSTPRunning())
                {
                    EVehicleStatus mRegistration = (EVehicleStatus)StopThePed.API.Functions.getVehicleRegistrationStatus(veh);
                    EVehicleStatus mInsurance = (EVehicleStatus)StopThePed.API.Functions.getVehicleInsuranceStatus(veh);

                    if (mRegistration == EVehicleStatus.Expired)
                        return EAlertType.Registration_Expired;
                    else if (mRegistration == EVehicleStatus.None)
                        return EAlertType.Unregistered_Vehicle;

                    if (mInsurance == EVehicleStatus.Expired)
                        return EAlertType.Insurance_Expired;
                    else if (mInsurance == EVehicleStatus.None)
                        return EAlertType.No_Insurance;
                }
            }

            return EAlertType.Null;
        }

        private EAlertType GenerateFlag(Vehicle veh)
        {
            if (!veh.Exists() || (!veh.HasDriver || !veh.Driver.Exists()))
            {
                return EAlertType.Null;
            }

            EAlertType mAlertToTrigger = EAlertType.Null;
            int alertFactor = mRand.Next(100);

            Dictionary<EAlertType, int> mAlertWeights = new Dictionary<EAlertType, int>() {
                { EAlertType.Stolen_Vehicle, Config.StolenVehicleWeight },
                { EAlertType.Unregistered_Vehicle, Config.UnregisteredVehicleWeight },
                { EAlertType.Registration_Expired, Config.RegistrationExpiredWeight },
                { EAlertType.No_Insurance, Config.NoInsuranceWeight },
                { EAlertType.Insurance_Expired, Config.InsuranceExpiredWeight },
                { EAlertType.UnTaxed, Config.UnTaxedWeight },
                { EAlertType.DrugsMarker, Config.DrugsMarkerWeight }
            };

            List<EAlertType> keys = (from x in mAlertWeights select x.Key).ToList();

            int cumulative = 0;
            foreach (var x in keys)
            {
                int mItemWeight = mAlertWeights[x];
                cumulative += mItemWeight;

                if (alertFactor < cumulative)
                {
                    mAlertToTrigger = x;
                    break;
                }
            }

            return mAlertToTrigger;
        }

        private API.ALPRScanResult CreateScanResult(Vehicle veh, EAlertType alert, ECamera cam)
        {
            API.ALPRScanResult r = null;

            switch (alert)
            {
                case EAlertType.Stolen_Vehicle:
                    r = CreateStolenVehResult(veh);
                    break;

                case EAlertType.Registration_Expired:
                    r = CreateRegExpiredResult(veh);
                    break;

                case EAlertType.Unregistered_Vehicle:
                    r = CreateRegNotValidResult(veh);
                    break;

                case EAlertType.No_Insurance:
                    r = CreateInsNotValidResult(veh);
                    break;

                case EAlertType.Insurance_Expired:
                    r = CreateInsExpiredResult(veh);
                    break;
                case EAlertType.UnTaxed:
                    r = CreateUntaxedResult(veh);
                    break;
                case EAlertType.DrugsMarker:
                    r = CreateDrugsMarkerResult(veh);
                    break;
            }

            API.Functions.RaiseALPRFlagGenerated(veh, new ALPREventArgs(r, cam));
            return r;
        }

        private API.ALPRScanResult CreateStolenVehResult(Vehicle veh)
        {
            API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.Stolen_Vehicle);
            string mRegisteredOwner = "";
            r.RegisteredOwner = mRegisteredOwner;
            veh.IsStolen = true;
            BlipHandler.Clear();
            BlipHandler.Attach(veh, Color.Red);
            return r;
        }

        private API.ALPRScanResult CreateRegNotValidResult(Vehicle veh)
        {
            API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.Unregistered_Vehicle);
            r.RegisteredOwner = "UNREGISTERED";
            Functions.SetVehicleOwnerName(veh, "UNREGISTERED");
            BlipHandler.Clear();
            BlipHandler.Attach(veh, Color.Red);
            StopThePed.API.Functions.setVehicleRegistrationStatus(veh, StopThePed.API.STPVehicleStatus.None);
            StopThePed.API.Functions.setVehicleInsuranceStatus(veh, StopThePed.API.STPVehicleStatus.None);
            return r;
        }

        private API.ALPRScanResult CreateRegExpiredResult(Vehicle veh)
        {
            API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.Registration_Expired);
            string mRegisteredOwner = "";
            r.RegisteredOwner = mRegisteredOwner;
            BlipHandler.Clear();
            BlipHandler.Attach(veh, Color.Yellow);
            StopThePed.API.Functions.setVehicleRegistrationStatus(veh, StopThePed.API.STPVehicleStatus.Expired);
            StopThePed.API.Functions.setVehicleInsuranceStatus(veh, StopThePed.API.STPVehicleStatus.Expired);
            return r;
        }

        private API.ALPRScanResult CreateInsNotValidResult(Vehicle veh)
        {
            API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.No_Insurance);
            string mRegisteredOwner = "";
            r.RegisteredOwner = mRegisteredOwner;
            BlipHandler.Clear();
            BlipHandler.Attach(veh, Color.Red);
            StopThePed.API.Functions.setVehicleRegistrationStatus(veh, StopThePed.API.STPVehicleStatus.None);
            StopThePed.API.Functions.setVehicleInsuranceStatus(veh, StopThePed.API.STPVehicleStatus.None);
            return r;
        }

        private API.ALPRScanResult CreateInsExpiredResult(Vehicle veh)
        {
            API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.Insurance_Expired);
            string mRegisteredOwner = "";
            r.RegisteredOwner = mRegisteredOwner;
            BlipHandler.Clear();
            BlipHandler.Attach(veh, Color.Yellow);
            StopThePed.API.Functions.setVehicleRegistrationStatus(veh, StopThePed.API.STPVehicleStatus.Expired);
            StopThePed.API.Functions.setVehicleInsuranceStatus(veh, StopThePed.API.STPVehicleStatus.Expired);
            return r;
        }

        private API.ALPRScanResult CreateUntaxedResult(Vehicle veh)
        {
            API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.UnTaxed);
            string mRegisteredOwner = "";
            r.RegisteredOwner = mRegisteredOwner;
            StopThePed.API.Functions.setVehicleRegistrationStatus(veh, StopThePed.API.STPVehicleStatus.Expired);
            StopThePed.API.Functions.setVehicleInsuranceStatus(veh, StopThePed.API.STPVehicleStatus.Expired);
            BlipHandler.Clear();
            BlipHandler.Attach(veh, Color.Red);
            return r;
        }
        private API.ALPRScanResult CreateDrugsMarkerResult(Vehicle veh)
        {
            API.ALPRScanResult r = new API.ALPRScanResult(veh, EAlertType.DrugsMarker);
            string mRegisteredOwner = "";
            r.RegisteredOwner = mRegisteredOwner;
            StopThePed.API.Functions.injectVehicleSearchItems(veh);
            StopThePed.API.Functions.injectPedSearchItems(veh.Driver);
            BlipHandler.Clear();
            BlipHandler.Attach(veh, Color.Blue);
            if (rand.Next(1, 5) == 4)
            {
                StopThePed.API.Functions.setPedUnderDrugsInfluence(veh.Driver, true);
            }
            if (rand.Next(1, 5) == 2)
            {
                StopThePed.API.Functions.setVehicleRegistrationStatus(veh, StopThePed.API.STPVehicleStatus.None);
                StopThePed.API.Functions.setVehicleInsuranceStatus(veh, StopThePed.API.STPVehicleStatus.None);
            }
            return r;
        }

        private void DisplayAlert(Vehicle veh, ECamera cam, API.ALPRScanResult r)
        {
            if (veh.Exists())
            {
                if (r.AlertType == EAlertType.Null)
                    return;

                r.LastDisplayed = DateTime.Now;

                if (!Globals.ScanResults.ContainsKey(veh))
                {
                    Globals.ScanResults.Add(veh, r);
                }
                else
                {
                    Globals.ScanResults[veh].LastDisplayed = DateTime.Now;
                }

                if (r.Persona != null)
                {
                    if (veh.HasDriver && veh.Driver.Exists())
                    {
                        Logger.LogTrivialDebug(String.Format("DisplayAlert() -- Setting Persona for driver (lic: {0}), (name: {1})", veh.LicensePlate, r.Persona.FullName));
                        Functions.SetPersonaForPed(veh.Driver, r.Persona);
                    }
                }

                if (r.RegisteredOwner != "")
                    Functions.SetVehicleOwnerName(veh, r.RegisteredOwner);

                string subtitle = "";

                switch (r.AlertType)
                {
                    case EAlertType.Stolen_Vehicle:
                        subtitle = "~r~STOLEN VEHCILE";
                        break;
                    case EAlertType.Registration_Expired:
                        subtitle = "~y~MOT Expired";
                        break;
                    case EAlertType.Unregistered_Vehicle:
                        subtitle = "~r~No MOT Held";
                        break;
                    case EAlertType.No_Insurance:
                        subtitle = "~r~No Insurance Held";
                        break;
                    case EAlertType.Insurance_Expired:
                        subtitle = "~y~Insurance Expired";
                        break;
                    case EAlertType.UnTaxed:
                        subtitle = "~r~No Tax Held";
                        break;
                    case EAlertType.DrugsMarker:
                        subtitle = "~r~Drugs Marker";
                        break;
                }

                if (r.IsCustomFlag)
                    subtitle = r.Result;

                string mColorName = "";
                try
                {
                    VehicleColor mColor = Stealth.Common.Natives.Vehicles.GetVehicleColors(veh);
                    mColorName = mColor.PrimaryColorName;
                }
                catch
                {
                    mColorName = "";
                }

                if (mColorName != "")
                    mColorName = mColorName + " ";

                string mTitle = String.Format("ANPR Hit[{0}]", GetCameraName(cam));
                string mVehModel = String.Format("{0}{1}", veh.Model.Name.Substring(0, 1).ToUpper(), veh.Model.Name.Substring(1).ToLower());
                string mText = String.Format("Plate: ~b~{0} ~n~~w~{1}{2}", veh.LicensePlate, mColorName, mVehModel);
                Funcs.DisplayNotification(mTitle, subtitle, mText);
                if (Config.PlayAlertSound)
                {
                    Audio.PlaySound(Audio.ESounds.TimerStop);
                }

                API.Functions.RaiseALPRResultDisplayed(veh, new ALPREventArgs(r, cam));
            }
        }

        private ECamera GetCapturingCamera(Vehicle veh)
        {
            if (veh.Exists())
            {
                float zDelta = Math.Abs(Globals.PlayerPed.Position.Z - veh.Position.Z);

                if (zDelta > 3)
                {
                    return ECamera.Null;
                }

                //Config.PassengerFrontAngle, hdgToVeh) && 
                //Config.PassengerRearAngle, hdgToVeh) && 
                //Config.DriverFrontAngle, hdgToVeh) && 
                //Config.DriverRearAngle, hdgToVeh) && 

                if (!IsVehicleInLineOfSight(veh))
                {
                    return ECamera.Null;
                }
                if (Config.EnablePassengerFrontCam)
                {
                    if (IsVehicleInCameraFOV(veh, Config.PassengerFrontAngle))
                    {
                        return ECamera.Passenger_Front;
                    }
                }

                if (Config.EnablePassengerRearCam)
                {
                    if (IsVehicleInCameraFOV(veh, Config.PassengerRearAngle))
                    {
                        return ECamera.Passenger_Rear;
                    }
                }
                if (Config.EnableDriverFrontCam)
                {
                    if (IsVehicleInCameraFOV(veh, Config.DriverFrontAngle))
                    {
                        return ECamera.Driver_Front;
                    }
                }
                if (Config.EnableDriverRearCam)
                {
                    if (IsVehicleInCameraFOV(veh, Config.DriverRearAngle))
                    {
                        return ECamera.Driver_Rear;
                    }
                }

                return ECamera.Null;
            }
            else
            {
                return ECamera.Null;
            }
        }

        private bool IsHeadingWithinFOV(int camAngle, float heading)
        {
            float min = MathHelper.NormalizeHeading(camAngle - (Config.CameraDegreesFOV / 2));
            float max = MathHelper.NormalizeHeading(camAngle + (Config.CameraDegreesFOV / 2));

            return heading.IsBetween(min, max);
        }

        private bool IsVehicleInLineOfSight(Vehicle veh)
        {
            try
            {
                if (!veh.Exists())
                {
                    return false;
                }

                Entity e;

                if (Globals.PlayerVehicle.Exists())
                    e = Globals.PlayerVehicle;
                else
                    e = Globals.PlayerPed;

                bool los = Rage.Native.NativeFunction.Natives.xFCDFF7B72D23A1AC<bool>(e, veh, 17); //HAS_ENTITY_CLEAR_LOS_TO_ENTITY

                //Logger.LogTrivialDebug("FOV check returning " + los.ToString());
                return los;
            }
            catch (Exception ex)
            {
                Logger.LogTrivialDebug("Error checking line of sight, assume false -- " + ex.ToString());
                return false;
            }
        }

        private bool IsVehicleInCameraFOV(Vehicle veh, int camAngle)
        {
            if (!veh.Exists() || !Globals.PlayerVehicle.Exists())
            {
                return false;
            }

            Vector3 playerVehPosition = Globals.PlayerVehicle.Position;
            float playerVehHeading = Globals.PlayerVehicle.Heading;
            float cameraHeading = playerVehHeading + camAngle;
            float cameraNormalizedHeading = MathHelper.NormalizeHeading(cameraHeading);

            Vector3 cameraDirection = MathHelper.ConvertHeadingToDirection(cameraNormalizedHeading);
            Vector3 cameraVector = cameraDirection * Config.CameraMinimum;

            Vector3 camPoint = playerVehPosition + cameraVector;

            Vector3 rearPlate = veh.GetOffsetPosition(Vector3.RelativeBack * 2f);
            Vector3 frontPlate = veh.GetOffsetPosition(Vector3.RelativeFront * 2f);

            Vector3 targetPoint = rearPlate;
            if (camPoint.DistanceTo(frontPlate) < camPoint.DistanceTo(rearPlate))
            {
                targetPoint = frontPlate;
            }

            float headingToTarget = camPoint.GetHeadingToPoint(targetPoint);
            bool isInFOV = IsHeadingWithinFOV(camAngle, headingToTarget);
            //Logger.LogTrivialDebug("FOV check returning " + isInFOV.ToString());

            return isInFOV;
        }

        private bool IsVehicleInCameraFOV2(Vehicle veh, int camAngle)
        {
            try
            {
                if (!veh.Exists() || !Globals.PlayerVehicle.Exists())
                {
                    return false;
                }
                //originPoint = Globals.PlayerVehicle.Position + (MathHelper.ConvertHeadingToDirection(MathHelper.NormalizeHeading(heading + camAngle)) * 1f);

                Vector3 originPoint = Globals.PlayerVehicle.Position;

                Vector3 playerVehPosition = Globals.PlayerVehicle.Position;
                float playerVehHeading = Globals.PlayerVehicle.Heading;
                float cameraHeading = playerVehHeading + camAngle;
                float cameraNormalizedHeading = MathHelper.NormalizeHeading(cameraHeading);

                Vector3 cameraDirection = MathHelper.ConvertHeadingToDirection(cameraNormalizedHeading);
                Vector3 cameraVector = cameraDirection * Config.CameraRange;

                Vector3 edgePoint = playerVehPosition + cameraVector;
                //Vector3 edgePoint = Globals.PlayerVehicle.Position + MathHelper.ConvertHeadingToDirection(MathHelper.NormalizeHeading(playerVehHeading + camAngle)) * Config.CameraRange;

                float camFOV = (float)Config.CameraDegreesFOV;

                bool isInFOV;

                try
                {
                    isInFOV = Rage.Native.NativeFunction.Natives.x51210CED3DA1C78A<bool>(veh, originPoint.X, originPoint.Y, originPoint.Z,
                    edgePoint.X, edgePoint.Y, edgePoint.Z, camFOV, 0, 1, 0); //IS_ENTITY_IN_ANGLED_AREA
                }
                catch
                {
                    isInFOV = false;
                }

                /*if (x < 4)
                {
                    GameFiber.StartNew(delegate
                    {
                        Blip b = new Blip(edgePoint);
                        b.Color = System.Drawing.Color.Yellow;

                        x += 1;

                        GameFiber.Sleep(5000);

                        if (b.Exists())
                        {
                            b.Delete();
                            x -= 1;
                        }
                    });
                }*/

                Logger.LogTrivialDebug("FOV check returning " + isInFOV.ToString());
                return isInFOV;
            }
            catch (Exception ex)
            {
                Logger.LogTrivialDebug("Error checking FOV, assume false -- " + ex.ToString());
                return false;
            }
        }

        internal bool IsActive(Vehicle veh)
        {
            if (veh.Exists())
            {
                if (Globals.ALPRVehicles.ContainsKey(veh))
                {
                    return Globals.ALPRVehicles[veh];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal bool IsVehicleAlreadyVerified(Vehicle veh)
        {
            if (veh.Exists())
            {
                if (Globals.ALPRVehicles.ContainsKey(veh))
                {
                    return true;
                }
                else
                {
                    Globals.ALPRVehicles.Add(veh, false);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal void ToggleActive(Vehicle veh)
        {
            if (veh.Exists() && Globals.ALPRVehicles.ContainsKey(veh))
            {
                if (!Globals.ALPRVehicles[veh])
                {
                    SetActive(veh, true);
                }
                else
                {
                    SetActive(veh, false);
                }
            }
        }

        internal void SetActive(Vehicle veh, bool active)
        {
            if (veh.Exists() && Globals.ALPRVehicles.ContainsKey(veh))
            {
                if (active)
                {
                    //enable
                    Globals.ALPRLastReadyOrActivation = DateTime.Now;
                    Globals.ALPRVehicles[veh] = true;
                    Funcs.DisplayNotification("~b~System Message", "System ~g~Activated");
                    Audio.PlaySound(Audio.ESounds.ThermalVisionOn);
                }
                else
                {
                    //disable
                    Globals.ALPRVehicles[veh] = false;
                    Funcs.DisplayNotification("~b~System Message", "System ~r~Deactivated");
                    Audio.PlaySound(Audio.ESounds.ThermalVisionOff);
                }
            }
        }

        private bool IsVehicleBlacklisted(Vehicle veh)
        {
            if (veh.Exists())
            {
                if (!(veh.IsCar | veh.IsBike) || veh.IsPoliceVehicle || mBlacklistedModels.Contains(veh.Model.Name) || (!veh.HasDriver || !veh.Driver.Exists()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private string GetCameraName(ECamera cam)
        {
            string mCamName = cam.ToFriendlyString();

            switch (cam)
            {
                case ECamera.Driver_Front:
                    mCamName = "Driver Front";
                    break;

                case ECamera.Driver_Rear:
                    mCamName = "Driver Rear";
                    break;

                case ECamera.Passenger_Rear:
                    mCamName = "Pass Rear";
                    break;

                case ECamera.Passenger_Front:
                    mCamName = "Pass Front";
                    break;

                default:
                    break;
            }

            return mCamName;
        }

        private List<string> mBlacklistedModels = new List<string>()
        {
            "boattrailer",
            "trailersmall",
            "trailers",
            "trailers2",
            "trailerlogs",
            "tr2",
            "docktrailer",
            "tanker",
            "AAVAN"
        };
    }
}
