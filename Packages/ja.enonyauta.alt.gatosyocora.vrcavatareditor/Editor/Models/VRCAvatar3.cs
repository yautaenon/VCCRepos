﻿#if VRC_SDK_VRCSDK3
using System;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRCAvatarEditor.Base;
using LipSyncStyle = VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle;
using AnimationSet = VRC.SDKBase.VRC_AvatarDescriptor.AnimationSet;
using Viseme = VRC.SDKBase.VRC_AvatarDescriptor.Viseme;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRCAvatarEditor.Utilities;

namespace VRCAvatarEditor.Avatars3
{
    public class VRCAvatar3 : VRCAvatarBase
    {
        public enum EyelidBlendShapes
        {
            Blink = 0,
            LookingUp = 1,
            LookingDown = 2
        }

        public VRCAvatarDescriptor Descriptor { get; set; }

        public CustomAnimLayer GestureLayer { get; set; }
        public int TargetGestureLayerIndex { get; set; }
        public AnimatorController GestureController { get; set; }

        public CustomAnimLayer FxLayer { get; set; }
        public int TargetFxLayerIndex { get; set; }
        public AnimatorController FxController { get; set; }

        public string[] EyelidBlendShapeNames { get; set; }
        public SkinnedMeshRenderer EyelidSkinnedMesh { get; set; }

        public VRCAvatar3() : base()
        {
            Descriptor = null;

            TargetGestureLayerIndex = 0;
            GestureController = null;

            TargetFxLayerIndex = 0;
            FxController = null;

            EyelidBlendShapeNames = new string[Enum.GetNames(typeof(EyelidBlendShapes)).Length];
            EyelidSkinnedMesh = null;
        }

        public VRCAvatar3(VRCAvatarDescriptor descriptor) : this()
        {
            if (descriptor == null) return;
            LoadAvatarInfo(descriptor);
        }

        public void LoadAvatarInfo()
        {
            LoadAvatarInfo(Descriptor);
        }

        public void LoadAvatarInfo(VRCAvatarDescriptor descriptor)
        {
            if (descriptor == null) return;
            Descriptor = descriptor;

            if (Descriptor.baseAnimationLayers != null)
            {
                GestureLayer = VRCAvatarAnimationUtility.GetPlayableLayer(Descriptor, AnimLayerType.Gesture);
                GestureController = GestureLayer.animatorController as AnimatorController;

                FxLayer = VRCAvatarAnimationUtility.GetPlayableLayer(Descriptor, AnimLayerType.FX);
                FxController = FxLayer.animatorController as AnimatorController;
            }

            if (FxController != null)
            {
                var layerNames = FxController.layers.Select(l => l.name).ToArray();
                TargetFxLayerIndex = Array.IndexOf(layerNames, VRCAvatarConstants.FX_LEFT_HAND_LAYER_NAME);

                if (TargetFxLayerIndex == -1)
                {
                    TargetFxLayerIndex = Array.IndexOf(layerNames, VRCAvatarConstants.FX_RIGHT_HAND_LAYER_NAME);
                    if (TargetFxLayerIndex == -1) TargetFxLayerIndex = 0;
                }
            }

            if (FxController != null && GestureController != null && TargetFxLayerIndex != -1)
            {
                var GestureLayerNames = GestureController.layers.Select(l => l.name).ToArray();
                var FxLayerNames = FxController.layers.Select(l => l.name).ToArray();

                TargetGestureLayerIndex = Array.IndexOf(GestureLayerNames, FxLayerNames[TargetFxLayerIndex]);
            }

            EyelidSkinnedMesh = Descriptor.customEyeLookSettings.eyelidsSkinnedMesh;
            if (EyelidSkinnedMesh != null)
            {
                var eyelidsFaceMesh = EyelidSkinnedMesh.sharedMesh;

                var settings = Descriptor.customEyeLookSettings;
                if (Descriptor.customEyeLookSettings.eyelidType == EyelidType.Blendshapes)
                {
                    var blinkBlendShapeIndex = settings.eyelidsBlendshapes[(int)EyelidBlendShapes.Blink];
                    if (blinkBlendShapeIndex != -1)
                    {
                        EyelidBlendShapeNames[(int)EyelidBlendShapes.Blink] = eyelidsFaceMesh.GetBlendShapeName(blinkBlendShapeIndex);
                    }

                    var lookingUpBlendShapeIndex = settings.eyelidsBlendshapes[(int)EyelidBlendShapes.LookingUp];
                    if (lookingUpBlendShapeIndex != -1)
                    {
                        EyelidBlendShapeNames[(int)EyelidBlendShapes.LookingUp] = eyelidsFaceMesh.GetBlendShapeName(lookingUpBlendShapeIndex);
                    }

                    var lookingDownBlendShapeIndex = settings.eyelidsBlendshapes[(int)EyelidBlendShapes.LookingDown];
                    if (lookingDownBlendShapeIndex != -1)
                    {
                        EyelidBlendShapeNames[(int)EyelidBlendShapes.LookingDown] = eyelidsFaceMesh.GetBlendShapeName(lookingDownBlendShapeIndex);
                    }
                }
            }

            AnimSavedFolderPath = GetAnimSavedFolderPath(FxController);
            FaceMesh = Descriptor.VisemeSkinnedMesh;

            if (FaceMesh != null && Descriptor.lipSync == LipSyncStyle.VisemeBlendShape)
            {
                LipSyncShapeKeyNames = new List<string>();
                LipSyncShapeKeyNames.AddRange(Descriptor.VisemeBlendShapes);
            }

            LipSyncStyle = Descriptor.lipSync;

            EyePos = Descriptor.ViewPosition;
            Sex = Descriptor.Animations;

            base.LoadAvatarInfo(Descriptor.gameObject);
        }

        public override void SetLipSyncToViseme()
        {
            if (Descriptor == null) return;

            LipSyncStyle = LipSyncStyle.VisemeBlendShape;
            Descriptor.lipSync = LipSyncStyle.VisemeBlendShape;

            if (FaceMesh == null)
            {
                FaceMesh = VRCAvatarMeshUtility.GetFaceMeshRenderer(this);
                Descriptor.VisemeSkinnedMesh = FaceMesh;
            }

            if (FaceMesh == null) return;
            var mesh = FaceMesh.sharedMesh;

            var visemeBlendShapeNames = Enum.GetNames(typeof(Viseme));

            if (Descriptor.VisemeBlendShapes == null || Descriptor.VisemeBlendShapes.Length <= 0)
            {
                Descriptor.VisemeBlendShapes = new string[VRCAvatarMeshUtility.LIPSYNC_SHYPEKEY_NUM];
            }

            for (int visemeIndex = 0; visemeIndex < visemeBlendShapeNames.Length; visemeIndex++)
            {
                // VRC用アバターとしてよくあるシェイプキーの名前を元に自動設定
                foreach (var prefix in VRCAvatarMeshUtility.lipSyncBlendShapeNamePrefixPatterns)
                {
                    var visemeShapeKeyName = prefix + visemeBlendShapeNames[visemeIndex];
                    if (mesh.GetBlendShapeIndex(visemeShapeKeyName) != -1)
                    {
                        Descriptor.VisemeBlendShapes[visemeIndex] = visemeShapeKeyName;
                        break;
                    }

                    visemeShapeKeyName = prefix + visemeBlendShapeNames[visemeIndex].ToLower();
                    if (mesh.GetBlendShapeIndex(visemeShapeKeyName) != -1)
                    {
                        Descriptor.VisemeBlendShapes[visemeIndex] = visemeShapeKeyName;
                        break;
                    }

                    visemeShapeKeyName = prefix + visemeBlendShapeNames[visemeIndex].ToUpper();
                    if (mesh.GetBlendShapeIndex(visemeShapeKeyName) != -1)
                    {
                        Descriptor.VisemeBlendShapes[visemeIndex] = visemeShapeKeyName;
                        break;
                    }

                    visemeShapeKeyName = prefix + visemeBlendShapeNames[visemeIndex] + " ";
                    if (mesh.GetBlendShapeIndex(visemeShapeKeyName) != -1)
                    {
                        Descriptor.VisemeBlendShapes[visemeIndex] = visemeShapeKeyName;
                        break;
                    }

                    // Shacloのvrc.v_ee用
                    visemeShapeKeyName = prefix + visemeBlendShapeNames[visemeIndex].ToLower();
                    visemeShapeKeyName += visemeShapeKeyName.Last();
                    if (mesh.GetBlendShapeIndex(visemeShapeKeyName) != -1)
                    {
                        Descriptor.VisemeBlendShapes[visemeIndex] = visemeShapeKeyName;
                        break;
                    }
                }
            }
        }

        public void SetAnimSavedFolderPath()
        {
            AnimSavedFolderPath = GetAnimSavedFolderPath(FxController);
        }
    }
}
#endif