using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CatNoirRing
{
    static class TimeStop
    {
        const float Step = 1f / 60f;

        sealed class HeldBody
        {
            public Rigidbody2D Body = null!;
            public RigidbodyConstraints2D Constraints;
            public Vector2 Velocity;
            public float Spin;
            public bool Lock;
        }

        sealed class HeldSound
        {
            public SoundHelper Helper = null!;
            public string Clip = "";
            public float Pitch;
            public bool Resume;
        }

        static readonly Dictionary<int, HeldBody> held = new Dictionary<int, HeldBody>();
        static readonly Dictionary<int, HeldSound> heldSounds = new Dictionary<int, HeldSound>();
        static readonly Dictionary<int, string> silencedClip = new Dictionary<int, string>();
        static readonly FieldInfo? audioField = AccessTools.Field(typeof(SoundHelper), "audioSource");
        static readonly FieldInfo? relationOwner = AccessTools.Field(typeof(Relationships), "agent");
        static readonly HashSet<int> stillItems = new HashSet<int>();
        static readonly HashSet<int> doorIds = new HashSet<int>();
        static readonly List<Rigidbody2D> props = new List<Rigidbody2D>();
        static readonly List<Agent> solo = new List<Agent>(2);
        static readonly List<SoundHelper> heard = new List<SoundHelper>();
        static readonly MethodInfo? travel = AccessTools.Method(typeof(Bullet), "travelForward");

        static List<Agent>? swappedList;
        static bool swapped;
        static float until;
        static float bank;
        static int clockDepth;
        static int audioHonest;
        static bool audioHeld;
        static bool catching;
        static bool simulateWarned;
        static bool soundWarned;
        static bool interactWarned;
        static AudioSource? music;
        static float musicPitch = 1f;
        static bool musicWasPlaying;
        static bool musicHeld;

        public static bool Active { get; private set; }
        public static Agent? Subject { get; private set; }
        public static bool PlayerClock { get; private set; }
        public static bool FixedStep { get; private set; }
        public static float ClockDelta { get; private set; }

        public static bool Begin(Agent agent)
        {
            if (Active || agent == null || agent.gc == null)
                return false;

            GameController gc = agent.gc;
            Subject = agent;
            until = Time.unscaledTime + Mathf.Max(1, CatNoirPlugin.TimeStopDuration.Value);
            bank = 0f;
            Snapshot(gc);
            Active = true;
            MuteWorld(gc);
            return true;
        }

        public static void End()
        {
            if (!Active && !audioHeld)
                return;

            Active = false;
            PlayerClock = false;
            FixedStep = false;
            clockDepth = 0;
            audioHonest = 0;
            Release();
            RestoreAudio();
            Subject = null;
            bank = 0f;
        }

        public static void Tick()
        {
            if (!Active)
                return;

            if (Subject == null || RabbitEffect.Find(Subject) == null || Time.unscaledTime >= until)
                End();
        }

        public static void PushClock(float delta)
        {
            if (clockDepth == 0)
                ClockDelta = delta;
            clockDepth++;
            PlayerClock = true;
        }

        public static void PopClock()
        {
            clockDepth = Math.Max(0, clockDepth - 1);
            PlayerClock = clockDepth > 0;
        }

        public static void Simulate(GameController gc)
        {
            if (!Active || gc == null || Subject == null || Subject.rb == null)
                return;

            if (HandsOff(gc, Subject))
            {
                Hold(gc);
                Subject.rb.velocity = Vector2.zero;
                Subject.rb.angularVelocity = 0f;
                return;
            }

            bank += Time.unscaledDeltaTime;
            int guard = 0;
            while (bank >= Step && guard < 5)
            {
                bank -= Step;
                guard++;
                Hold(gc);
                FixedStep = true;
                PushClock(Step);
                try
                {
                    Subject.AgentFixedUpdate();
                    if (!Subject.localPlayer && !Subject.dead && !Subject.fakeDead && Subject.movement != null)
                        Subject.movement.PlayerMovement();
                    Animator? body = Subject.agentSpriteAnimator;
                    if (body != null && body.isActiveAndEnabled)
                        body.Update(Step);
                    MoveShots(gc);
                    Physics2D.Simulate(Step);
                }
                catch (Exception exception)
                {
                    if (!simulateWarned)
                    {
                        simulateWarned = true;
                        Debug.LogWarning("Остановка времени: шаг кролика не прошёл.");
                        Debug.LogWarning(exception);
                    }
                }
                finally
                {
                    PopClock();
                    FixedStep = false;
                }
            }

            if (bank > Step * 5f)
                bank = 0f;
        }

        static bool HandsOff(GameController gc, Agent player)
        {
            if (gc.menuGUI != null && gc.menuGUI.onMenu)
                return true;
            if (gc.playerControl != null && gc.playerControl.cantPressButtons)
                return true;
            if (player.mainGUI == null)
                return false;

            return player.mainGUI.openedInventory
                || player.mainGUI.openedCharacterSheet
                || player.mainGUI.openedQuestSheet
                || player.mainGUI.openedBigImage
                || player.mainGUI.openedCharacterCreation
                || player.mainGUI.openedScrollingMenu
                || player.mainGUI.openedScrollingMenuPersonal
                || player.mainGUI.openedSetSeed
                || (gc.mainGUI != null && gc.mainGUI.openedCharacterSelect);
        }

        static void Snapshot(GameController gc)
        {
            held.Clear();
            stillItems.Clear();
            props.Clear();
            if (gc.itemList != null)
            {
                for (int i = 0; i < gc.itemList.Count; i++)
                {
                    Item item = gc.itemList[i];
                    if (item != null)
                        stillItems.Add(item.GetInstanceID());
                }
            }

            List<ObjectReal>? things = gc.objectRealList;
            if (things == null)
                return;

            for (int i = 0; i < things.Count; i++)
            {
                ObjectReal thing = things[i];
                if (thing == null)
                    continue;
                Rigidbody2D body = thing.GetComponent<Rigidbody2D>();
                if (body != null)
                    props.Add(body);
            }
        }

        static void Hold(GameController gc)
        {
            if (Subject == null)
                return;

            int self = Subject.GetInstanceID();
            if (gc.agentList != null)
            {
                for (int i = 0; i < gc.agentList.Count; i++)
                {
                    Agent agent = gc.agentList[i];
                    if (agent == null || agent.GetInstanceID() == self || agent.rb == null)
                        continue;
                    Lock(agent.rb);
                }
            }

            if (gc.bulletList != null)
            {
                for (int i = 0; i < gc.bulletList.Count; i++)
                {
                    Bullet shot = gc.bulletList[i];
                    if (shot == null || shot.agent == Subject)
                        continue;
                    Lock(shot.GetComponent<Rigidbody2D>());
                }
            }

            if (gc.itemList != null)
            {
                for (int i = 0; i < gc.itemList.Count; i++)
                {
                    Item item = gc.itemList[i];
                    if (item == null || item.rb == null || !stillItems.Contains(item.GetInstanceID()))
                        continue;
                    Stall(item.rb);
                }
            }

            for (int i = 0; i < props.Count; i++)
                Stall(props[i]);
        }

        static void Lock(Rigidbody2D? body)
        {
            if (body == null || Subject == null || body == Subject.rb)
                return;

            int id = body.GetInstanceID();
            if (!held.TryGetValue(id, out HeldBody saved))
            {
                saved = new HeldBody
                {
                    Body = body,
                    Constraints = body.constraints,
                    Velocity = body.velocity,
                    Spin = body.angularVelocity,
                    Lock = true,
                };
                held[id] = saved;
            }

            saved.Lock = true;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        static void Stall(Rigidbody2D? body)
        {
            if (body == null || Subject == null || body == Subject.rb)
                return;

            int id = body.GetInstanceID();
            if (held.TryGetValue(id, out HeldBody saved))
            {
                if (saved.Lock)
                    return;
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
                return;
            }

            held[id] = new HeldBody
            {
                Body = body,
                Constraints = body.constraints,
                Velocity = body.velocity,
                Spin = body.angularVelocity,
                Lock = false,
            };
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        static void Release()
        {
            foreach (HeldBody saved in held.Values)
            {
                if (saved.Body == null)
                    continue;
                saved.Body.constraints = saved.Constraints;
                saved.Body.velocity = saved.Velocity;
                saved.Body.angularVelocity = saved.Spin;
            }

            held.Clear();
            stillItems.Clear();
            props.Clear();
        }

        static void MuteWorld(GameController gc)
        {
            heldSounds.Clear();
            silencedClip.Clear();
            CatchWorld(gc, true);
            HoldMusic(gc);
            audioHeld = true;
        }

        static void CatchWorld(GameController? gc, bool remember)
        {
            if (catching)
                return;

            List<SoundHelper>? list = gc != null ? gc.soundHelperList : null;
            if (list == null)
                return;

            catching = true;
            try
            {
                for (int i = 0; i < list.Count; i++)
                    HoldSound(list[i], remember);
            }
            finally
            {
                catching = false;
            }
        }

        static void HoldSound(SoundHelper? helper, bool remember)
        {
            if (helper == null || !IsWorld(helper))
                return;

            AudioSource? source = AudioOf(helper);
            if (source == null)
                return;

            int id = helper.GetInstanceID();
            string clip = helper.clipName ?? "";
            if (silencedClip.TryGetValue(id, out var done) && done == clip)
                return;

            if (remember && !heldSounds.ContainsKey(id))
            {
                heldSounds[id] = new HeldSound
                {
                    Helper = helper,
                    Clip = clip,
                    Pitch = PitchOf(source),
                    Resume = Playing(source),
                };
            }

            silencedClip[id] = clip;
            try
            {
                if (Playing(source))
                    Pause(source);
            }
            catch (Exception exception)
            {
                if (!soundWarned)
                {
                    soundWarned = true;
                    Debug.LogWarning("Остановка времени: звук мира не замер.");
                    Debug.LogWarning(exception);
                }
            }
        }

        static void RestoreAudio()
        {
            if (!audioHeld && heldSounds.Count == 0)
                return;

            foreach (HeldSound saved in heldSounds.Values)
            {
                if (saved.Helper == null || (saved.Helper.clipName ?? "") != saved.Clip)
                    continue;

                AudioSource? source = AudioOf(saved.Helper);
                if (source == null)
                    continue;

                source.pitch = saved.Pitch;
                if (saved.Resume)
                    Unpause(source);
            }

            heldSounds.Clear();
            silencedClip.Clear();
            RestoreMusic();
            audioHeld = false;
        }

        static void HoldMusic(GameController gc)
        {
            AudioSource? player = gc.musicPlayer;
            if (player == null)
                return;

            music = player;
            musicPitch = player.pitch;
            musicWasPlaying = player.isPlaying;
            musicHeld = true;
            if (player.isPlaying)
                player.Pause();
        }

        static void RestoreMusic()
        {
            if (!musicHeld || music == null)
            {
                musicHeld = false;
                music = null;
                return;
            }

            music.pitch = musicPitch;
            if (musicWasPlaying)
                music.UnPause();
            musicHeld = false;
            music = null;
        }

        static AudioSource? AudioOf(SoundHelper helper)
        {
            return audioField?.GetValue(helper) as AudioSource;
        }

        static bool Playing(AudioSource source)
        {
            return source != null && source.isPlaying;
        }

        static float PitchOf(AudioSource source)
        {
            return source != null ? source.pitch : 1f;
        }

        static void Pause(AudioSource source)
        {
            if (source != null && source.isPlaying)
                source.Pause();
        }

        static void Unpause(AudioSource source)
        {
            if (source != null)
                source.UnPause();
        }

        static bool IsWorld(SoundHelper helper)
        {
            return helper != null && !HeardByHero(helper);
        }

        static bool HeardByHero(SoundHelper helper)
        {
            Agent? subject = Subject;
            if (subject == null)
                return false;

            Transform root = subject.tr != null ? subject.tr : subject.transform;
            if (helper.transform != null && helper.transform.IsChildOf(root))
                return true;

            PlayfieldObject? source = helper.playfieldObject;
            if (source == null || source == subject)
                return source == subject;

            switch (source)
            {
                case Bullet bullet:
                    return bullet.agent == subject;
                case Melee melee:
                    return melee.agent == subject;
                case Explosion blast:
                    return blast.agent == subject;
                case Fire fire:
                    return fire.agent == subject;
                case Item item:
                    return item.owner == subject || item.thrower == subject || item.throwerReal == subject;
                default:
                    return false;
            }
        }

        static void HearHero(GameController gc)
        {
            List<SoundHelper>? list = gc.soundHelperList;
            if (list == null)
                return;

            heard.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                SoundHelper helper = list[i];
                if (helper != null && !IsWorld(helper))
                    heard.Add(helper);
            }

            if (heard.Count == 0)
                return;

            PushClock(Time.unscaledDeltaTime);
            try
            {
                for (int i = 0; i < heard.Count; i++)
                {
                    if (heard[i] == null)
                        continue;
                    try
                    {
                        heard[i].UpdateSoundHelper();
                    }
                    catch (Exception exception)
                    {
                        if (!soundWarned)
                        {
                            soundWarned = true;
                            Debug.LogWarning("Остановка времени: звук кролика не прошёл.");
                            Debug.LogWarning(exception);
                        }
                    }
                }
            }
            finally
            {
                PopClock();
            }
        }

        static void DriveSubject(GameController gc)
        {
            Agent? agent = Subject;
            if (agent == null)
                return;

            InteractionHelper? helper = agent.interactionHelper;
            if (helper == null)
                return;

            bool restoreLocal = false;
            if (!agent.localPlayer)
            {
                agent.localPlayer = true;
                restoreLocal = true;
            }

            bool pushed = false;
            try
            {
                PushClock(Time.unscaledDeltaTime);
                pushed = true;
                helper.UpdateInteractionHelper();
                TickAim(helper);
            }
            catch (Exception exception)
            {
                if (!interactWarned)
                {
                    interactWarned = true;
                    Debug.LogWarning("Остановка времени: действие кролика не прошло.");
                    Debug.LogWarning(exception);
                }
            }
            finally
            {
                if (pushed)
                    PopClock();
                if (restoreLocal && agent != null)
                    agent.localPlayer = false;
            }
        }

        static void TickAim(InteractionHelper helper)
        {
            doorIds.Clear();
            TickReal(helper.interactionObjectReal);

            Item item = helper.interactionItem;
            if (item != null)
                item.UpdateItem();
        }

        static void TickReal(ObjectReal? real)
        {
            if (real == null || real.destroyed)
                return;
            if (!doorIds.Add(real.GetInstanceID()))
                return;

            real.ObjectUpdate();
        }

        static bool HeroSource(AudioSource source)
        {
            if (source == null || Subject == null)
                return false;

            GameController gc = GameController.gameController;
            if (gc == null || gc.musicPlayer == source)
                return false;

            List<SoundHelper>? list = gc.soundHelperList;
            if (list == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                SoundHelper helper = list[i];
                if (helper == null || AudioOf(helper) != source)
                    continue;
                return HeardByHero(helper);
            }

            return false;
        }

        static bool WorldSource(AudioSource source)
        {
            if (source == null)
                return false;

            GameController gc = GameController.gameController;
            if (gc == null)
                return false;
            if (gc.musicPlayer == source)
                return true;

            List<SoundHelper>? list = gc.soundHelperList;
            if (list == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                SoundHelper helper = list[i];
                if (helper != null && AudioOf(helper) == source && IsWorld(helper))
                    return true;
            }

            return false;
        }

        static void LateShots(GameController gc)
        {
            if (gc.bulletList == null || Subject == null)
                return;

            PushClock(Time.unscaledDeltaTime);
            try
            {
                for (int i = gc.bulletList.Count - 1; i >= 0; i--)
                {
                    if (i >= gc.bulletList.Count)
                        continue;
                    Bullet shot = gc.bulletList[i];
                    if (shot == null || shot.agent != Subject || !shot.gameObject.activeInHierarchy)
                        continue;
                    shot.LateUpdateBullet();
                }
            }
            catch (Exception exception)
            {
                if (!simulateWarned)
                {
                    simulateWarned = true;
                    Debug.LogWarning("Остановка времени: шаг кролика не прошёл.");
                    Debug.LogWarning(exception);
                }
            }
            finally
            {
                PopClock();
            }
        }

        static void MoveShots(GameController gc)
        {
            if (travel == null || gc.bulletList == null || Subject == null)
                return;

            for (int i = 0; i < gc.bulletList.Count; i++)
            {
                Bullet shot = gc.bulletList[i];
                if (shot == null || shot.agent != Subject || !shot.gameObject.activeInHierarchy)
                    continue;
                try
                {
                    travel.Invoke(shot, null);
                }
                catch (Exception)
                {
                }
            }
        }

        static void BorrowAgents(GameController gc)
        {
            if (!Active || swapped || gc == null)
                return;

            swapped = true;
            swappedList = gc.agentList;
            solo.Clear();
            if (Subject != null)
                solo.Add(Subject);
            gc.agentList = solo;
        }

        static void ReturnAgents(GameController gc)
        {
            if (!swapped)
                return;

            swapped = false;
            if (gc != null && swappedList != null)
                gc.agentList = swappedList;
            swappedList = null;
        }

        [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
        static class DeltaPatch
        {
            static bool Prefix(ref float __result)
            {
                if (!PlayerClock)
                    return true;

                __result = ClockDelta;
                return false;
            }
        }

        [HarmonyPatch(typeof(Time), nameof(Time.fixedDeltaTime), MethodType.Getter)]
        static class FixedPatch
        {
            static bool Prefix(ref float __result)
            {
                if (!PlayerClock || !FixedStep)
                    return true;

                __result = ClockDelta;
                return false;
            }
        }

        [HarmonyPatch(typeof(Time), nameof(Time.timeScale), MethodType.Getter)]
        static class ScalePatch
        {
            static bool Prefix(ref float __result)
            {
                if (audioHonest > 0)
                    return true;
                if (!PlayerClock)
                    return true;

                __result = 1f;
                return false;
            }
        }

        [HarmonyPatch(typeof(Updater), "Update")]
        static class AfterWorld
        {
            static void Postfix()
            {
                if (!Active)
                    return;

                GameController gc = GameController.gameController;
                if (gc == null)
                    return;

                CatchWorld(gc, false);
                Hold(gc);
                MoveShots(gc);
            }
        }

        [HarmonyPatch(typeof(Updater), nameof(Updater.UpdateAgents))]
        static class LiveAgents
        {
            static void Prefix()
            {
                BorrowAgents(GameController.gameController);
            }

            static void Finalizer()
            {
                ReturnAgents(GameController.gameController);
            }
        }

        [HarmonyPatch(typeof(Updater), nameof(Updater.LateUpdateAgents))]
        static class LiveLateAgents
        {
            static void Prefix()
            {
                BorrowAgents(GameController.gameController);
            }

            static void Finalizer()
            {
                ReturnAgents(GameController.gameController);
            }
        }

        [HarmonyPatch(typeof(Updater), nameof(Updater.FixedUpdateAgents))]
        static class LiveFixedAgents
        {
            static void Prefix()
            {
                BorrowAgents(GameController.gameController);
            }

            static void Finalizer()
            {
                ReturnAgents(GameController.gameController);
            }
        }

        [HarmonyPatch(typeof(Updater), nameof(Updater.UpdateObjectReals))]
        [HarmonyPatch(typeof(Updater), nameof(Updater.UpdateItems))]
        [HarmonyPatch(typeof(Updater), nameof(Updater.UpdateOther))]
        [HarmonyPatch(typeof(Updater), nameof(Updater.LateUpdateObjectReals))]
        [HarmonyPatch(typeof(Updater), nameof(Updater.LateUpdateOther))]
        static class FrozenWorld
        {
            static bool Prefix()
            {
                return !Active;
            }
        }

        [HarmonyPatch(typeof(CameraScript), "Update")]
        static class LiveCamera
        {
            static void Prefix()
            {
            }

            static void Finalizer()
            {
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "Update")]
        static class LiveInput
        {
            static void Prefix()
            {
            }

            static void Finalizer()
            {
            }
        }

        [HarmonyPatch(typeof(tk2dSpriteAnimator), "LateUpdate")]
        static class LiveSprites
        {
            static bool Prefix(tk2dSpriteAnimator __instance)
            {
                if (!Active)
                    return true;

                Agent agent = __instance.GetComponentInParent<Agent>();
                if (agent != null && agent == Subject)
                    __instance.UpdateAnimation(Time.unscaledDeltaTime);
                return false;
            }
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.SetTimeScale))]
        static class KeepScale
        {
            static void Prefix(GameController __instance, ref bool __state)
            {
                __state = false;
            }

            static void Finalizer(GameController __instance, bool __state)
            {
            }
        }

        [HarmonyPatch(typeof(AudioHandler), nameof(AudioHandler.PlayClipAt))]
        static class QuietNew
        {
            static void Postfix()
            {
                if (!Active)
                    return;

                CatchWorld(GameController.gameController, false);
            }
        }

        static bool OthersFrozen(Agent? agent)
        {
            return Active && agent != null && agent != Subject;
        }

        [HarmonyPatch(typeof(InteractionHelper), nameof(InteractionHelper.UpdateInteractionHelper))]
        static class SubjectInteract
        {
            static void Postfix(InteractionHelper __instance)
            {
                if (!Active || __instance == null || Subject == null || Subject.interactionHelper != __instance || __instance.interactionAgent == null)
                    return;

                __instance.interactionAgent = null;
                GameObject body = __instance.interactionObject;
                if (body != null && body.GetComponent<Agent>() != null)
                    __instance.interactionObject = null;
                WorldSpaceGUI talk = Subject.worldSpaceGUI;
                if (talk != null)
                    talk.HideObjectButtons();
            }
        }

        [HarmonyPatch(typeof(Agent), "OnCollisionEnter2D")]
        static class StillPeople
        {
            static bool Prefix(Agent __instance, Collision2D other)
            {
                return !OthersFrozen(__instance);
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.Interact))]
        static class NoTalk
        {
            static bool Prefix(Agent __instance)
            {
                return !OthersFrozen(__instance);
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.Say), typeof(string))]
        [HarmonyPatch(typeof(Agent), nameof(Agent.Say), typeof(string), typeof(bool))]
        static class NoSpeech
        {
            static bool Prefix(Agent __instance)
            {
                return !OthersFrozen(__instance);
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.SayDialogue), typeof(string))]
        [HarmonyPatch(typeof(Agent), nameof(Agent.SayDialogue), typeof(bool), typeof(string))]
        [HarmonyPatch(typeof(Agent), nameof(Agent.SayDialogue), typeof(string), typeof(bool))]
        [HarmonyPatch(typeof(Agent), nameof(Agent.SayDialogue), typeof(string), typeof(bool), typeof(uint))]
        [HarmonyPatch(typeof(Agent), nameof(Agent.SayDialogue), typeof(bool), typeof(string), typeof(bool))]
        [HarmonyPatch(typeof(Agent), nameof(Agent.SayDialogue), typeof(bool), typeof(string), typeof(bool), typeof(uint))]
        static class NoLine
        {
            static bool Prefix(Agent __instance, ref string __result)
            {
                if (!OthersFrozen(__instance))
                    return true;

                __result = "";
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayfieldObject), nameof(PlayfieldObject.ShowObjectButtons))]
        static class NoPrompt
        {
            static bool Prefix(PlayfieldObject __instance)
            {
                return !(__instance is Agent agent && OthersFrozen(agent));
            }
        }

        [HarmonyPatch(typeof(Relationships), nameof(Relationships.UpdateRelationships))]
        static class NoSight
        {
            static bool Prefix(Relationships __instance)
            {
                if (!Active || __instance == null)
                    return true;

                Agent? owner = relationOwner?.GetValue(__instance) as Agent;
                return !OthersFrozen(owner);
            }
        }

        [HarmonyPatch(typeof(Agent), "AgentUpdate")]
        [HarmonyPatch(typeof(Agent), "AgentLateUpdate")]
        [HarmonyPatch(typeof(Agent), "AgentFixedUpdate")]
        static class FreezeMind
        {
            static bool Prefix(Agent __instance)
            {
                return !OthersFrozen(__instance);
            }
        }

        [HarmonyPatch(typeof(Gun), "GunUpdate")]
        [HarmonyPatch(typeof(Gun), "GunLateUpdate")]
        static class FreezeGun
        {
            static bool Prefix(Gun __instance)
            {
                return __instance == null || !OthersFrozen(__instance.agent);
            }
        }

        [HarmonyPatch(typeof(Melee), "CheckAttack", typeof(bool))]
        [HarmonyPatch(typeof(Melee), "Attack", typeof(bool))]
        static class FreezeSwing
        {
            static bool Prefix(Melee __instance)
            {
                return __instance == null || !OthersFrozen(__instance.agent);
            }
        }

        [HarmonyPatch(typeof(StatusEffects), "StatusEffectsUpdate")]
        static class FreezeStatus
        {
            static bool Prefix(StatusEffects __instance)
            {
                return __instance == null || !OthersFrozen(__instance.agent);
            }
        }

        [HarmonyPatch(typeof(Gun), "Shoot", typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPatch(typeof(Gun), "Shoot", typeof(bool), typeof(bool), typeof(bool), typeof(int))]
        [HarmonyPatch(typeof(Gun), "Shoot", typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(string))]
        static class FreezeShot
        {
            static bool Prefix(Gun __instance)
            {
                return __instance == null || !OthersFrozen(__instance.agent);
            }
        }

        [HarmonyPatch(typeof(Relationships), nameof(Relationships.LastSaw))]
        static class FreezeSight
        {
            static bool Prefix(Relationships __instance)
            {
                if (!Active || __instance == null)
                    return true;

                Agent? owner = relationOwner?.GetValue(__instance) as Agent;
                return !OthersFrozen(owner);
            }
        }

        [HarmonyPatch(typeof(Relationships), nameof(Relationships.SetRel), typeof(Agent), typeof(string))]
        [HarmonyPatch(typeof(Relationships), nameof(Relationships.SetRel), typeof(Agent), typeof(string), typeof(bool))]
        static class FreezeAnger
        {
            static bool Prefix(Relationships __instance)
            {
                if (!Active || __instance == null)
                    return true;

                Agent? owner = relationOwner?.GetValue(__instance) as Agent;
                return !OthersFrozen(owner);
            }
        }

        [HarmonyPatch(typeof(InvItem), nameof(InvItem.UseItem))]
        static class FreezeDrink
        {
            static bool Prefix(InvItem __instance)
            {
                return __instance == null || !OthersFrozen(__instance.agent);
            }
        }

        [HarmonyPatch(typeof(ConveyorBelt), "OnTriggerStay2D")]
        [HarmonyPatch(typeof(Laser), "OnTriggerEnter2D")]
        [HarmonyPatch(typeof(Mine), "ObjectCollide")]
        [HarmonyPatch(typeof(Mine), "ObjectCollideStay")]
        [HarmonyPatch(typeof(Danger), "OnTriggerEnter2D")]
        [HarmonyPatch(typeof(Gas), "OnTriggerEnter2D")]
        [HarmonyPatch(typeof(FlameGrate), nameof(FlameGrate.Trap))]
        [HarmonyPatch(typeof(SawBlade), "ObjectCollide")]
        static class FrozenHazards
        {
            static bool Prefix()
            {
                return !Active;
            }
        }

        [HarmonyPatch(typeof(Fire), "OnTriggerEnter2D")]
        static class FrozenFire
        {
            static bool Prefix(Fire __instance)
            {
                if (!Active)
                    return true;
                return __instance != null && __instance.agent == Subject;
            }
        }

        [HarmonyPatch(typeof(Explosion), "OnTriggerEnter2D")]
        [HarmonyPatch(typeof(Explosion), "OnTriggerStay2D")]
        static class FrozenBlast
        {
            static bool Prefix(Explosion __instance)
            {
                if (!Active)
                    return true;
                return __instance != null && __instance.agent == Subject;
            }
        }

        [HarmonyPatch(typeof(MeleeHitbox), "HitObject")]
        static class FrozenSwing
        {
            static bool Prefix(MeleeHitbox __instance)
            {
                if (!Active)
                    return true;
                Melee? melee = __instance != null ? __instance.myMelee : null;
                return melee != null && melee.agent == Subject;
            }
        }

        [HarmonyPatch(typeof(BulletHitbox), "HitObject")]
        static class FrozenShot
        {
            static bool Prefix(BulletHitbox __instance)
            {
                if (!Active)
                    return true;
                Bullet? bullet = __instance != null ? __instance.myBullet : null;
                return bullet != null && bullet.agent == Subject;
            }
        }
    }
}
