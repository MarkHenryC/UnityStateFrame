using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QS
{
    public static class Utils
    {
        private static readonly RaycastHit[] physicsRaycasts = new RaycastHit[5];
        private static Ray ray = new Ray();

        private static readonly bool dampingInited;
        private static int lowPassSamples;
        private static QData[] qSamples;

        private const float TouchpadMinYForward = 0.2f;
        private const float TouchpadMaxYBackward = -0.9f;
        private const double TWOPI = 6.2831853071795865;
        private const double RAD2DEG = 57.2957795130823209;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public static string SEP = @"\";
        private static readonly string RAW_VIDEO_PATH = @"%LOCALAPPDATA%\BusyMarketing\";
#else
        public static string SEP = @"/";
        private static string RAW_VIDEO_PATH = "/storage/emulated/0/BusyMarketing/";
#endif
        private static string _resolvedVideoPath;

        public class QData
        {
            public Vector3 forward, up;
            public QData(Vector3 f, Vector3 u)
            {
                forward = f;
                up = u;
            }

            public QData() : this(new Vector3(), new Vector3())
            {

            }

            public Quaternion ToQ()
            {
                return Quaternion.LookRotation(forward, up);
            }
        }

        public static string VIDEO_PATH
        {
            get
            {
                if (string.IsNullOrEmpty(_resolvedVideoPath))
                {
                    string pathTo = System.Environment.ExpandEnvironmentVariables(RAW_VIDEO_PATH);
                    _resolvedVideoPath = System.IO.Path.GetFullPath(pathTo);
                    if (!_resolvedVideoPath.EndsWith(SEP))
                        _resolvedVideoPath += SEP;
                }
                return _resolvedVideoPath;
            }
        }

        public static string ResolvedPath(string fileName)
        {
            return VIDEO_PATH + fileName;
        }

        public static string GetFirstMkvVideo()
        {
            DirectoryInfo di = new DirectoryInfo(Utils.VIDEO_PATH);
            if (di != null && di.Exists)
            {
                FileSystemInfo[] fisArray = di.GetFileSystemInfos();
                foreach (FileSystemInfo fis in fisArray)
                {
                    if (fis.Attributes != FileAttributes.Directory && fis.Extension == ".mkv")
                        return fis.Name;
                }
            }
            return null;
        }

        /// <summary>
        /// Defaults to next state unless nextActivity is set
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="continueButton"></param>
        /// <param name="nextActivity"></param>
        public static void OptSkipButton(string prompt, ButtonPanel continueButton, bool nextActivity = false)
        {
            if (continueButton)
            {
                if (ActivitySettings.Asset.AllowSkipScenes)
                {
                    // Won't work without pointer
                    ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.Pointing;

                    continueButton.SetTrigger(prompt, () =>
                    {
                        ActivityManager.Instance.FadeOutThen(() => { ActivityManager.Instance.Next(nextActivity); });
                    });
                }
                else
                    continueButton.Show(false);
            }
        }

        public static float UnitValueChallengeScore(int score, int total)
        {
            return score / (float)total;
        }

        public static int ConvertToActivityScore(float unitValueScore)
        {
            return Mathf.RoundToInt(ActivitySettings.pointsPerChallenge * unitValueScore);
        }

        /// <summary>
        /// Keep support for chef experience
        /// </summary>
        /// <param name="score"></param>
        public static void RegisterActivityScore(int score)
        {
            ActivitySettings.Asset.currentExperienceTotalScore += score;
            ActivitySettings.Asset.currentExperienceMaxValue += ActivitySettings.pointsPerChallenge;
        }

        /// <summary>
        /// Simplified version for later experiences. Call only once per activity.
        /// Intended for end of single-run activities.
        /// </summary>
        /// <param name="rawScore"></param>
        public static void RegisterActivityAndUpdateExperience(int rawScore)
        {
            UpdateActivityPoints(rawScore);
            UpdateExperienceScores();
        }

        /// <summary>
        /// This is idempotent so can be called by
        /// replayable activities. Call each time is played.
        /// </summary>
        /// <param name="rawScore"></param>
        public static void UpdateActivityPoints(int rawScore)
        {
            int score = Mathf.Max(rawScore, 0);
            int bonus = 0;
            if (score > ActivitySettings.pointsPerChallenge)
            {
                bonus = rawScore - ActivitySettings.pointsPerChallenge;
                score = ActivitySettings.pointsPerChallenge;
            }
            ActivitySettings.Asset.currentActivityScore = score;
            ActivitySettings.Asset.currentActivityBonusPoints = bonus;
        }

        /// <summary>
        ///  For later versions which update currentActivity.
        ///  Ensures score is not less than zero and calculates bonus
        ///  based on overflow
        /// </summary>
        /// <param name="rawScore"></param>
        /// <param name="score"></param>
        /// <param name="bonus"></param>
        public static int CalcScoreAndBonus(int rawScore, out int bonus)
        {
            int score = Mathf.Max(rawScore, 0);
            bonus = 0;
            if (score > ActivitySettings.pointsPerChallenge)
            {
                bonus = rawScore - ActivitySettings.pointsPerChallenge;
                score = ActivitySettings.pointsPerChallenge;
            }
            return score;
        }

        /// <summary>
        /// For restarting an activity
        /// </summary>
        public static void ClearActivityPoints()
        {
            ActivitySettings.Asset.currentActivityScore = 0;
            ActivitySettings.Asset.currentActivityBonusPoints = 0;
        }

        /// <summary>
        /// Assumes the current activity scores are up to date.
        /// Only call this once per activity!
        /// </summary>
        public static void UpdateExperienceScores()
        {
            ActivitySettings.Asset.currentExperienceTotalScore += ActivitySettings.Asset.currentActivityScore;
            ActivitySettings.Asset.currentExperienceBonusPoints += ActivitySettings.Asset.currentActivityBonusPoints;
            ActivitySettings.Asset.currentExperienceMaxValue += ActivitySettings.pointsPerChallenge;
        }

        /// <summary>
        /// Assumption: 1 is max useful deduction.
        /// </summary>
        /// <param name="deduction"></param>
        /// <returns></returns>
        public static int GetDeductionPoints(float deduction)
        {
            deduction = Mathf.Clamp01(Mathf.Max(deduction, 0));
            int pointsOff = (int)(ActivitySettings.pointsPerChallenge * deduction);

            return pointsOff;
        }

        public static FixedJoint JointToRigidbody(GameObject jointObject, GameObject bodyObject, bool addComponents = true)
        {
            FixedJoint joint = jointObject.GetComponent<FixedJoint>();
            if (addComponents && !joint)
            {
                joint = jointObject.AddComponent<FixedJoint>();
            }

            if (joint)
            {
                Rigidbody rb = bodyObject.GetComponent<Rigidbody>();
                if (addComponents && !rb)
                    rb = bodyObject.AddComponent<Rigidbody>();

                if (rb)
                    joint.connectedBody = rb;
            }

            return joint;
        }

        public static void FaceAwayFromCamera(Transform obj, Transform camera = null)
        {
            if (!camera)
                camera = Camera.main.transform;
            obj.LookAt(new Vector3(camera.position.x, obj.position.y, camera.position.z));
        }

        /// <summary>
        /// Call a given action for each component
        /// of type T within parent transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="action"></param>
        public static void Each<T>(Transform parent, Action<T> action) where T : Component
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform t = parent.GetChild(i);
                T comp = t.GetComponent<T>();
                if (comp)                
                    action(comp);                
            }
        }

        /// <summary>
        /// Extract an array of component type T
        /// from immediate children of Transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T[] GetArrayOf<T>(Transform parent) where T : Component
        {
            int count = GetComponentCount<T>(parent);
            T[] components = new T[count];
            int componentIndex = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform t = parent.GetChild(i);
                T comp = t.GetComponent<T>();
                if (comp)
                    components[componentIndex++] = comp;
            }
            return components;
        }

        /// <summary>
        /// Count how many components of type T
        /// are children of given transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static int GetComponentCount<T>(Transform parent) where T : Component
        {            
            int componentCount = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform t = parent.GetChild(i);
                T comp = t.GetComponent<T>();
                if (comp)
                    componentCount++;
            }
            return componentCount;
        }

        /// <summary>
        /// Usually for text, where the forward
        /// vector is actually the back of the text. Also
        /// for objects, like Pourables, so we pick them
        /// up with a known orientation
        /// </summary>
        /// <param name="t"></param>
        public static void FaceCamera(Transform obj, Transform camera = null)
        {
            FaceAwayFromCamera(obj, camera);
            obj.Rotate(0, 180f, 0);
        }

        /// <summary>
        /// Place ab object such as a widget or text panel
        /// in front of the camera
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="placementLength"></param>
        /// <param name="camera"></param>
        public static void PlaceBeforeCamera(Transform obj, float placementLength, Transform camera = null)
        {
            if (!camera)
                camera = Camera.main.transform;
            Vector3 dest = camera.position + camera.forward * placementLength;
            obj.transform.position = dest;
            FaceCamera(obj, camera);
        }

        public static void PlaceInfoPanelBeforeCamera(Transform obj, float placementLength, Transform camera = null)
        {
            if (!camera)
                camera = Camera.main.transform;
            Vector3 dest = camera.position + camera.forward * placementLength;
            dest.y += ActivitySettings.Asset.infoPanelYOffset;
            obj.transform.position = dest;
            FaceCamera(obj, camera);
        }

        public static void PlaceInfoPanelCloseBeforeCamera(Transform obj, float placementLength, Transform camera = null)
        {
            if (!camera)
                camera = Camera.main.transform;
            Vector3 dest = camera.position + camera.forward * placementLength;

            obj.transform.position = dest;
            FaceCamera(obj, camera);
        }

        public static void TravelAlongPath(GameObject item, Vector3[] path, float time = 1.5f, 
            bool orientToPath = true, bool hideAtEnd = false, System.Action<GameObject> callOnComplete = null)
        {
            LeanTween.sequence().append(() =>
            {
                LeanTween.moveSpline(item, path, time)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOrientToPath(orientToPath);
            }).append(time).append(() =>
            {
                item.SetMode(!hideAtEnd);
                callOnComplete?.Invoke(item);
            });
        }

        /// <summary>
        /// Useful for returning a dropped object to
        /// its original position and orientation
        /// </summary>
        /// <param name="Graspable"></param>
        /// <param name="destination"></param>
        /// <param name="rotation"></param>
        /// <param name="time"></param>
        public static void MoveToPosition(Graspable Graspable, Vector3 destination, Quaternion rotation, float time)
        {
            Graspable.Dormant = true; // Don't allow hits
            Graspable.EnableRb(false);
            LeanTween.sequence().append(() =>
            {
                LeanTween.move(Graspable.gameObject, destination, time);
            }).append(() =>
            {
                Graspable.gameObject.transform.rotation = rotation;
                Graspable.EnableRb(true);
                Graspable.Dormant = false;
            });
        }

        public static void MoveToPositionImmediate(Graspable Graspable, Vector3 destination, Quaternion rotation)
        {
            MoveToPosition(Graspable, destination, rotation, 0.05f);
        }

        /// <summary>
        /// For dropping a Placeable into a pot
        /// </summary>
        /// <param name="Graspable"></param>
        /// <param name="destination"></param>
        /// <param name="time"></param>
        public static void MoveToPositionThenHideAtPosition(Graspable Graspable, Vector3 destination, Vector3 hidePosition, float time)
        {
            Graspable.Dormant = true; // Don't allow hits
            Graspable.EnableRb(false);
            LeanTween.sequence().append(() =>
            {
                LeanTween.move(Graspable.gameObject, destination, time);
            }).append(time).append(() =>
            {
                Graspable.EnableRb(true);
                Graspable.Show(false);
                Graspable.transform.position = hidePosition;
            });
        }

        public static void MoveToPositionAlongPath(Graspable Graspable, Vector3[] path, float time = 1.5f, bool orientToPath = true)
        {
            Graspable.Dormant = true; // Don't allow hits
            Graspable.EnableRb(false);

            LeanTween.moveSpline(Graspable.gameObject, path, time)
                .setEase(LeanTweenType.easeOutQuad)
                .setOrientToPath(orientToPath);
        }

        public static void RotateTo(Graspable Graspable, Vector3 eulerAngles, float time = 1.5f)
        {
            Graspable.Dormant = true; // Don't allow hits
            Graspable.EnableRb(false);

            LeanTween.rotate(Graspable.gameObject, eulerAngles, time)
                .setEase(LeanTweenType.easeOutQuad);
        }

        /// <summary>
        /// Same as move and hide but with path
        /// </summary>
        /// <param name="Graspable"></param>
        /// <param name="path"></param>
        /// <param name="time"></param>
        public static void MoveToPositionAlongPathThenHide(Graspable Graspable, Vector3[] path, float time = 1.5f, bool orientToPath = true)
        {
            Graspable.Dormant = true; // Don't allow hits
            Graspable.EnableRb(false);

            LeanTween.moveSpline(Graspable.gameObject, path, time)
                .setEase(LeanTweenType.easeOutQuad)
                .setOrientToPath(orientToPath)
                .setOnComplete(() =>
                {
                    Graspable.EnableRb(true);
                    Graspable.Show(false);
                });
        }

        /// <summary>
        /// For dropping a Placeable into a pot
        /// </summary>
        /// <param name="Graspable"></param>
        /// <param name="destination"></param>
        /// <param name="time"></param>
        public static void MoveToPositionAndHide(Graspable Graspable, Vector3 destination, float time)
        {
            Graspable.Dormant = true; // Don't allow hits
            Graspable.EnableRb(false);
            LeanTween.sequence().append(() =>
            {
                LeanTween.move(Graspable.gameObject, destination, time);
            }).append(time).append(() =>
            {
                Graspable.EnableRb(true);
                Graspable.Show(false);
                Graspable.Dormant = true; // Don't need dormant flag if disabled
            });
        }

        /// <summary>
        /// This was based on a 'notch' controller, which is
        /// probably undesirable for smooth navigation
        /// </summary>
        /// <param name="processedVrEventInfo"></param>
        /// <param name="targetPoint"></param>
        /// <param name="guidePoint"></param>
        /// <returns></returns>
        public static bool OrigHandleUserNavigation(VrEventInfo processedVrEventInfo, ref Vector3 targetPoint, ref Vector3 guidePoint)
        {
            Vector3 controllerDir = processedVrEventInfo.ControllerDirection.normalized;
            float dot = Vector3.Dot(controllerDir, Vector3.up);

            if (dot > -.75f && dot < .75f) // Originally .5f
            {

                Vector3 playerPos = ControllerInput.Instance.PlayerRb.position;
                Vector3 padDirection = new Vector3(processedVrEventInfo.TouchpadPosition.x, 1, processedVrEventInfo.TouchpadPosition.y);

                VrEventInfo.VrArrowKey direction = processedVrEventInfo.GetArrowKey();

                switch (direction)
                {
                    case VrEventInfo.VrArrowKey.Up:
                        guidePoint = processedVrEventInfo.ControllerPosition + controllerDir * 2f;
                        break;
                    case VrEventInfo.VrArrowKey.Down:
                        guidePoint = processedVrEventInfo.ControllerPosition + Quaternion.AngleAxis(180, Vector3.up) * controllerDir * 2f;
                        break;
                    case VrEventInfo.VrArrowKey.Right:
                        guidePoint = processedVrEventInfo.ControllerPosition + Quaternion.AngleAxis(90, Vector3.up) * controllerDir * 2f;
                        break;
                    case VrEventInfo.VrArrowKey.Left:
                        guidePoint = processedVrEventInfo.ControllerPosition + Quaternion.AngleAxis(-90, Vector3.up) * controllerDir * 2f;
                        break;
                    default:
                        return false;
                }

                guidePoint.y = playerPos.y;

                targetPoint = Vector3.MoveTowards(playerPos, guidePoint, ActivitySettings.Asset.playerSpeed * Time.fixedDeltaTime);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// See ControllerInput.FreeMove for explanation
        /// </summary>
        /// <param name="processedVrEventInfo"></param>
        /// <returns></returns>
        public static Vector3 deprecated_GetOneAxisMovement(VrEventInfo processedVrEventInfo)
        {
            Vector3 controllerDir = processedVrEventInfo.ControllerDirection.normalized;
            Vector3 playerPos = ControllerInput.Instance.PlayerRb.position;

            Vector3 targetPoint = playerPos + controllerDir * ActivitySettings.Asset.playerSpeed * processedVrEventInfo.TouchpadPosition.y;
            targetPoint.y = playerPos.y;

            return targetPoint;
        }

        public static Vector3 GetOneAxisMovement(VrEventInfo processedVrEventInfo)
        {
            Vector3 controllerDir = processedVrEventInfo.ControllerDirection.normalized;
            Vector3 playerPos = ControllerInput.Instance.PlayerRb.position;

            Vector3 targetPoint = playerPos + controllerDir * ActivitySettings.Asset.playerSpeed;
            targetPoint.y = playerPos.y;

            return targetPoint;
        }

        /// <summary>
        /// For a more analogue feel. Works 4-way continuous,
        /// bit a bit hard to control. And probably don't need
        /// or want strafing
        /// </summary>
        /// <param name="processedVrEventInfo"></param>
        /// <returns></returns>
        public static bool HandleUserNavigation(VrEventInfo processedVrEventInfo, out Vector3 targetPoint, out float travelAngle, out Vector3 guidePoint)
        {
            Vector3 controllerDir = processedVrEventInfo.ControllerDirection.normalized;
            float dot = Vector3.Dot(controllerDir, Vector3.up);
            travelAngle = 0f;
            guidePoint = Vector3.zero;
            targetPoint = Vector3.zero;

            if (dot > -.75f && dot < .75f) // Originally .5f
            {
                Vector3 playerPos = ControllerInput.Instance.PlayerRb.position;
                travelAngle = GetTravelAngle(processedVrEventInfo);
                Debug.Log("Travel angle: " + travelAngle);
                guidePoint = processedVrEventInfo.ControllerPosition + Quaternion.AngleAxis(travelAngle, Vector3.up) * controllerDir * 2f;
                guidePoint.y = playerPos.y;

                targetPoint = Vector3.MoveTowards(playerPos, guidePoint, ActivitySettings.Asset.playerSpeed * Time.fixedDeltaTime);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// First check there's no x,y == 0,0 touchpad reading
        /// before calling this
        /// </summary>
        /// <param name="processedVrEventInfo"></param>
        /// <returns></returns>
        public static float GetTravelAngle(VrEventInfo processedVrEventInfo)
        {
            double theta = Mathf.Atan2(processedVrEventInfo.TouchpadPosition.x, processedVrEventInfo.TouchpadPosition.y);
            if (theta < 0.0)
                theta += TWOPI;
            return (float)(RAD2DEG * theta);
        }

        /// <summary>
        /// Controller rotation goes from 0-360 CCW.
        /// So: 
        /// left turn is 0..threshold
        /// right turn is 360..(360-threshold).
        /// Return joystick range -1 (full left) to 1 (full right)
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static float GetUnitRotationZ(Quaternion rotation, float threshold = 45f)
        {
            float rotZ = Mathf.Abs(rotation.eulerAngles.z % 360f);
            if (rotZ > 180f)
            {
                float rightRotationCapped = Mathf.Min(threshold, 360f - rotZ);
                return rightRotationCapped / threshold;
            }
            else
            {
                float leftRotationCapped = Mathf.Min(threshold, rotZ);
                return -leftRotationCapped / threshold;
            }
        }

        public static bool DowncastToTarget(Vector3 startPoint, LayerMask mask, float distance = 1f)
        {
            if (Physics.RaycastNonAlloc(startPoint, Vector3.down, physicsRaycasts, distance, mask) > 0)
                return true;
            else
                return false;
        }

        public static T[] GetNonExcludedActivities<T>(Transform t) where T : ActivityBase
        {
            List<T> components = new List<T>();

            for (int i = 0; i < t.childCount; i++)
            {
                T a = t.GetChild(i).GetComponent<T>();
                if (a && !a.exclude)
                    components.Add(a);
            }

            return components.ToArray();
        }

        public static T[] GetNonExcludedStates<T>(Transform t) where T : StateProcessor
        {
            List<T> components = new List<T>();

            for (int i = 0; i < t.childCount; i++)
            {
                T a = t.GetChild(i).GetComponent<T>();
                if (a && !a.exclude)
                    components.Add(a);
            }

            return components.ToArray();
        }

        public static T[] GetActiveSubComponents<T>(Transform t) where T : Component
        {
            List<T> components = new List<T>();

            for (int i = 0; i < t.childCount; i++)
            {
                T a = t.GetChild(i).GetComponent<T>();
                if (a && t.gameObject.activeInHierarchy)
                    components.Add(a);
            }

            return components.ToArray();
        }

        public static Vector3 KeepY(Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        /// <summary>
        /// This one allows arbitrary start from a Transform
        /// </summary>
        /// <param name="start"></param>
        /// <param name="waypoints"></param>
        /// <returns></returns>
        public static Vector3[] SimpleSplinePath(Transform start, Transform[] waypoints)
        {
            List<Vector3> waypointList = new List<Vector3>
            {
                start.position, // Make control point same as start point
                start.position
            };
            foreach (Transform w in waypoints)
                waypointList.Add(w.position);
            waypointList.Add(waypoints[waypoints.Length - 1].position); // ditto with last control point
            return waypointList.ToArray();
        }

        public static Vector3[] SimpleSplinePath(Transform[] waypoints)
        {
            List<Vector3> waypointList = new List<Vector3>
            {
                waypoints[0].position, // Make control point same as start point
            };

            foreach (Transform w in waypoints)
                waypointList.Add(w.position);
            waypointList.Add(waypoints[waypoints.Length - 1].position); // ditto with last control point
            return waypointList.ToArray();
        }

        /// <summary>
        /// For a commonly-used LeanTween anim:
        /// move something from its current transform
        /// via waypoint transforms, not changing Y.
        /// Since we only need an even curve, we just
        /// make the beginning and end control points
        /// equal to the first and last positions.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="waypoints"></param>
        /// <returns></returns>
        public static Vector3[] SimpleXZSplinePath(Transform start, Transform[] waypoints, bool loop = false)
        {
            List<Vector3> waypointList = new List<Vector3>
            {
                start.position, // Make control point same as start point
                start.position
            };

            float y = start.position.y;
            foreach (Transform w in waypoints)
                waypointList.Add(Utils.KeepY(w.position, y));

            if (loop)
            {
                waypointList.Add(Utils.KeepY(start.position, y));
                waypointList.Add(Utils.KeepY(start.position, y)); // for bezier alignment
            }
            else
                waypointList.Add(Utils.KeepY(waypoints[waypoints.Length - 1].position, y)); // ditto with last control point

            return waypointList.ToArray();
        }

        public static Vector3[] SimpleSplinePath(Transform[] waypoints, bool loop = false)
        {
            List<Vector3> waypointList = new List<Vector3>();

            if (loop)
                waypointList.Add(waypoints[waypoints.Length - 1].position);
            else
                waypointList.Add(waypoints[0].position);

            foreach (Transform w in waypoints)
                waypointList.Add(w.position);

            if (loop)
            {
                waypointList.Add(waypoints[0].position);
                waypointList.Add(waypoints[1].position);
            }
            else
                waypointList.Add(waypoints[waypoints.Length - 1].position);

            return waypointList.ToArray();
        }

        public static Vector3[] WaypointsFromStart(Transform start, Transform[] waypoints)
        {
            List<Vector3> waypointList = new List<Vector3>
            {
                waypoints[waypoints.Length-1].position,
                start.position
            };

            for (int i = 0; i < waypoints.Length; i++)
                waypointList.Add(waypoints[i].position);

            waypointList.Add(start.position);

            return waypointList.ToArray();
        }

        public static bool InRangeInclusive(int number, Vector2 range)
        {
            return number >= range.x && number <= range.y;
        }

        public static float GetVolume(GameObject g)
        {
            Renderer r = g.GetComponent<Renderer>();
            if (r)
            {
                Bounds bounds = r.bounds;
                return bounds.size.x * bounds.size.y * bounds.size.z;
            }
            return 0f;
        }

        public static float AbsRotationUnitVal(float startAngle, float currentDegrees, bool ccw = true)
        {
            float absDelta = Mathf.Abs(currentDegrees - startAngle);
            float normalized = 0f;

            if (absDelta >= 180f) // must have crossed over
            {
                Debug.LogFormat("Crossover. Start: {0}, current: {1}", startAngle, currentDegrees);
                if (currentDegrees < startAngle)
                    normalized = (currentDegrees + (360f - startAngle)) / 360f;
                else
                    normalized = -((360f - currentDegrees) + startAngle) / 360f;
            }
            else
                normalized = (currentDegrees - startAngle) / 360f;

            return ccw ? normalized : -normalized;
        }

        /// <summary>
        /// Range -1 .. 1
        /// </summary>
        /// <param name="previousDegrees"></param>
        /// <param name="currentDegrees"></param>
        /// <param name="ccw"></param>
        /// <returns></returns>
        public static float DeltaRotationUnitVal(float previousDegrees, float currentDegrees, bool ccw = true)
        {
            float absDelta = Mathf.Abs(currentDegrees - previousDegrees);
            float normalized = 0f;

            if (absDelta >= 180f) // must have crossed over
            {
                //Debug.LogFormat("Crossover - previous: {0}, current: {1}", previousDegrees, currentDegrees);

                if (currentDegrees < previousDegrees)
                    normalized = (currentDegrees + (360f - previousDegrees)) / 360f;
                else
                    normalized = -((360f - currentDegrees) + previousDegrees) / 360f;
            }
            else
                normalized = (currentDegrees - previousDegrees) / 360f;

            return ccw ? normalized : -normalized;
        }

        public static float GetUnitValue(float maxAmount, float achievedAmount, bool ceiling = false)
        {
            if (ceiling)
                achievedAmount = Mathf.Min(maxAmount, achievedAmount);

            return achievedAmount / maxAmount;
        }

        public static int GetPercentage(float maxAmount, float achievedAmount, bool ceiling = false)
        {
            float ratio = GetUnitValue(maxAmount, achievedAmount, ceiling);
            return (int)(ratio * 100f);
        }

        public static float UnitValueClamped(int percent)
        {
            return Mathf.Min(percent / 100f, 1f);
        }

        public static int MakePercentage(float unitValue)
        {
            return (int)(unitValue * 100f);
        }

        public static Component ExclusiveActivate(Component[] components, string chosenOne)
        {
            Component chosen = null;
            foreach (Component component in components)
            {
                if (component.name == chosenOne)
                {
                    chosen = component;
                    chosen.gameObject.SetActive(true);
                }
                else
                    component.gameObject.SetActive(false);
            }
            return chosen;
        }

        /// <summary>
        /// Have an issue here: it never gets a hit on the Graspable and
        /// always (in the case of something on the floor like a broom)
        /// returns the floor collider. Haven't figured out what's going
        /// on here, as it hits just fine in the main raycast routing
        /// when it picks up the Graspable component. No idea why an
        /// independent raycast doesn't get a hit.
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static bool DirectLineToObject(Graspable g)
        {
            ray.origin = ControllerInput.Instance.ControllerPosition;
            ray.direction = (g.transform.position - ray.origin).normalized;

            if (Physics.RaycastNonAlloc(ray, physicsRaycasts, ActivitySettings.Asset.raycastDistance) > 0)
            {
                //Debug.DrawRay(ray.origin, ray.direction.normalized * ActivitySettings.Asset.raycastDistance, Color.yellow, 5f);

                Debug.Log("Direct line to: " + physicsRaycasts[0].collider.name);

                if (physicsRaycasts[0].collider.gameObject == g)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Won't work in this project if it has a rigidbody attached. See alt version below
        /// </summary>
        /// <returns></returns>
        public static GameObject FirstHit()
        {
            ray.origin = ControllerInput.Instance.ControllerPosition;
            ray.direction = ControllerInput.Instance.ControllerDirection;

            if (Physics.RaycastNonAlloc(ray, physicsRaycasts, ActivitySettings.Asset.raycastDistance) > 0)
            {
                //Debug.DrawRay(ray.origin, ray.direction.normalized * ActivitySettings.Asset.raycastDistance, Color.yellow, 5f);

                Debug.Log("First hit: " + physicsRaycasts[0].collider.name);

                return physicsRaycasts[0].collider.gameObject;
            }
            return null;
        }

        public static string OneOrMore(int quantity, string single, string multiple)
        {
            return (Mathf.Abs(quantity) == 1 ? single : multiple);
        }

        public static string Minutised(float seconds)
        {
            if (seconds >= 60f)
            {
                int minutes = Mathf.FloorToInt(seconds / 60f);
                float remainder = seconds - (minutes * 60f);
                return string.Format("{0} {1} and {2}", minutes, OneOrMore(minutes, "minute", "minutes"), remainder);
            }
            else
                return string.Format("{0}", seconds);
        }

        /// <summary>
        /// Smooth a rotation such as controller to
        /// prevent jitter. Single use only, as it
        /// uses a static buffer for efficiency
        /// </summary>
        /// <param name="rot"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static Quaternion SmoothQuaternion(Quaternion rot, int samples)
        {
            if (samples != lowPassSamples)
                InitQData(samples);

            Vector3 fwdAvg = rot * Vector3.forward;
            Vector3 upAvg = rot * Vector3.up;

            Vector3 fwdAdd = fwdAvg;
            Vector3 upAdd = upAvg;

            for (int i = 1; i < lowPassSamples; i++)
            {
                qSamples[i - 1].forward = qSamples[i].forward;
                qSamples[i - 1].up = qSamples[i].up;

                fwdAvg += qSamples[i - 1].forward;
                upAvg += qSamples[i - 1].up;
            }

            qSamples[lowPassSamples - 1].forward = fwdAdd;
            qSamples[lowPassSamples - 1].up = upAdd;

            return new QData(fwdAvg.normalized, upAvg.normalized).ToQ();
        }

        /// <summary>
        /// Smoothing from start to end. 0 is no smoothing (returns target value), 1 is infinite (returns source)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="smoothing">0 is no smoothing (returns target value), 1 is infinite (returns source)</param>
        /// <param name="deltaTime">as supplied by Unity</param>
        /// <returns></returns>
        public static Quaternion SmoothQuaternion(Quaternion start, Quaternion end, float smoothing, float deltaTime)
        {
            return Quaternion.Lerp(start, end, 1 - Mathf.Pow(smoothing, deltaTime));
        }

        /// <summary>
        /// Smoothing from source to target. 0 is no smoothing (returns target value), 1 is infinite (returns source)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="smoothing">0 is no smoothing (returns target value), 1 is infinite (returns source)</param>
        /// <param name="deltaTime">as supplied by Unity</param>
        /// <returns></returns>
        public static float Damp(float source, float target, float smoothing, float deltaTime)
        {
            return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, deltaTime));
        }

        /// <summary>
        /// Vector version
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="smoothing"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 Damp(Vector3 source, Vector3 target, float smoothing, float deltaTime)
        {
            return Vector3.Lerp(source, target, 1 - Mathf.Pow(smoothing, deltaTime));
        }

        /// <summary>
        /// S-shaped smooth. t 0..1
        /// </summary>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float EaseInOut(float t, float f)
        {
            return t * t * t * (t * (6f * t - 15f) + 10f);
        }

        /// <summary>
        /// Update samples by shifting left and adding 
        /// new value at head
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="newVal"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float BucketShiftLeft(float[] bucket, float newVal, int size)
        {
            float avg = newVal;

            for (int i = 1; i < size; i++)
            {
                bucket[i - 1] = bucket[i];
                avg += bucket[i - 1];
            }

            bucket[size - 1] = newVal;

            return avg / size;
        }

        /// <summary>
        /// Prepare static buffer for Quaternion smoother
        /// </summary>
        /// <param name="size"></param>
        private static void InitQData(int size)
        {
            lowPassSamples = size;
            qSamples = new QData[lowPassSamples];
            for (int i = 0; i < lowPassSamples; i++)
                qSamples[i] = new QData();
        }

        /// <summary>
        /// Concatenate the LookRotation with the rot around 
        /// the z axis of the object - in that order.
        /// Keeps object facing player and rotates around its
        /// z axis
        /// </summary>
        public static Quaternion ZRotateFacingController(Vector3 objPos, float zRot)
        {
            return Quaternion.LookRotation(objPos - ControllerInput.Instance.ControllerPosition) *
                Quaternion.AngleAxis(zRot, Vector3.forward);
        }

        public static T PopRandomItemFromList<T>(ref List<T> list)
        {
            int len = list.Count;
            if (len > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, len);
                T item = list[randomIndex];
                list.RemoveAt(randomIndex);
                return item;
            }
            else
                return default;
        }

        /// <summary>
        /// Darken colour unitVal amount (0-1f)
        /// Alpha set to source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="unitVal"></param>
        /// <returns></returns>
        public static Color ShadeColor(Color source, float unitVal)
        {
            return new Color(source.r * (1f - unitVal),
                             source.g * (1f - unitVal),
                             source.b * (1f - unitVal),
                             source.a);
        }

        /// <summary>
        /// Lighten Colour unitVal amount (0-1f)
        /// Alpha set to source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="unitVal"></param>
        /// <returns></returns>
        public static Color TintColor(Color source, float unitVal)
        {
            return new Color(source.r + (1f - source.r) * unitVal,
                             source.g + (1f - source.g) * unitVal,
                             source.b + (1f - source.b) * unitVal,
                             source.a);
        }

        /// <summary>
        /// Blend a colour with alpha. Blend alpha scales blend.
        /// Output alpha set to source alpha.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="blend"></param>
        /// <returns></returns>
        public static Color BlendColor(Color source, Color blend)
        {
            return new Color(source.r + (blend.r - source.r) * blend.a,
                             source.g + (blend.g - source.g) * blend.a,
                             source.b + (blend.b - source.b) * blend.a,
                             source.a);
        }

        /// <summary>
        /// Blends and normalizes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceAmount"></param>
        /// <param name="blend"></param>
        /// <param name="blendAmount"></param>
        /// <returns></returns>
        public static Color BlendColor(Color source, float sourceAmount, Color blend, float blendAmount)
        {
            return new Color(
                NormalizedBlend(source.r * source.a, sourceAmount, blend.r * blend.a, blendAmount),
                NormalizedBlend(source.g * source.a, sourceAmount, blend.g * blend.a, blendAmount),
                NormalizedBlend(source.b * source.a, sourceAmount, blend.b * blend.a, blendAmount));
        }

        private static float NormalizedBlend(float c1, float amt1, float c2, float amt2)
        {
            float normalize = amt1 + amt2;
            if (normalize == 0f)
                return 0f;
            return (c1 * amt1 + c2 * amt2) / normalize;

        }

        public static void ShuffleList<T>(ref T[] target)
        {
            for (int i = 0; i < target.Length; i++)
            {
                var temp = target[i];
                var ix = UnityEngine.Random.Range(0, target.Length);
                target[i] = target[ix];
                target[ix] = temp;
            }
        }

        public static Vector3[] SphericalPointArray(int nPoints, float fScale, Vector3 vPosition)
        {
            float fp = nPoints;
            float inc = Mathf.PI * (3f - Mathf.Sqrt(5f));
            float off = 2f / fp;

            Vector3[] pts = new Vector3[nPoints];

            for (int i = 0; i < nPoints; i++)
            {
                float y = (float)i * off - 1f + (off / 2f);
                float r = Mathf.Sqrt(1f - y * y);
                float phi = (float)i * inc;
                pts[i] = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
                pts[i] *= fScale;
                pts[i] += vPosition;
            }

            return pts;
        }

        public static bool IsNear(Vector3 a, Vector3 b, float prox)
        {
            return Vector3.Distance(a, b) < prox;
        }

        public static bool IsNearFast(Vector3 a, Vector3 b, float prox)
        {
            return Vector3.SqrMagnitude(b - a) < (prox * prox);
        }

    }
}