// Old Skull Games
// Bernard Barthelemy
// Friday, April 26, 2019

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OSG.ConfigHelper
{
    [Serializable]
    internal class Config
    {
        [SerializeField] public string name;
        [SerializeField] private string defines;
        [HideInInspector][SerializeField] private List<Inclusion> elementsToInclude;
        [SerializeField] public BuildTargetGroup targetGroup;

        public void Apply(TargetConfigHelper helper)
        {
            Debug.Log("Applying " + name);
            if(targetGroup != EditorUserBuildSettings.selectedBuildTargetGroup)
            {
                Debug.LogWarning("Wrong target selected, please select " + targetGroup);
            }

            var elementsToProcess = new List<Element>(helper.elements);
            Includes(elementsToProcess);
            Excludes(elementsToProcess);
            ApplyDefines(helper);
        }

        private void Excludes(List<Element> elements)
        {
            foreach (Element element in elements)
            {
                element.Exclude();
            }
        }

        private void Includes(List<Element> elementsToProcess)
        {
            foreach (var inclusion in elementsToInclude)
            {
                inclusion.Include(elementsToProcess, this);
            }
        }

        private void ApplyDefines(TargetConfigHelper config)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                targetGroup == BuildTargetGroup.Unknown
                    ? EditorUserBuildSettings.selectedBuildTargetGroup
                    : targetGroup,
                string.IsNullOrEmpty(defines)
                    ? config.commonDefines
                    : $"{config.commonDefines};{defines}");
        }

        public Inclusion Uses(Element element)
        {
            return elementsToInclude.Find(i => i.elementGUID == element.guid);
        }

        public const string common = "COMMON";
        public string GetArchiveAbsolutePath(Element element, bool useOverride)
        {
            return DirectoryHelpers.Combine(ArchiveRoot, useOverride ? name : common, element.assetRelativePath);
        }

        public static string ArchiveRoot => DirectoryHelpers.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6),"Archives");

        public void UpdateArchive(Element element)
        {
            Inclusion inclusion = Uses(element);
            if(inclusion==null || ! inclusion.overrideArchivePath)
                return;

            bool update = EditorUtility.DisplayDialog("Config " + name,
                $"Update {element.assetRelativePath} included in {name}?", "yes", "no");
            if(update)
            {
                string archivePath = GetArchiveAbsolutePath(element, true);
                element.Copy(element.AssetAbsolutePath, archivePath);
            }
        }

        public void Remove(Element element)
        {
            for(int index = elementsToInclude.Count;--index>=0;)
            {
                if(elementsToInclude[index].elementGUID == element.guid)
                {
                    elementsToInclude.RemoveAt(index);
                    break;
                }
            }
        }

        private Inclusion AddUnique(Element element)
        {
            var inclusion = Uses(element);
            if (inclusion == null)
            {
                inclusion = new Inclusion(element);
                elementsToInclude.Add(inclusion);
            }
            return inclusion;
        }


        public void UseCommon(Element element)
        {
            Inclusion isUsing = AddUnique(element);
            isUsing.overrideArchivePath = false;
        }

        public void UseOwn(Element element)
        {
            Inclusion isUsing = AddUnique(element);
            isUsing.overrideArchivePath = true;
            string ownArchive = GetArchiveAbsolutePath(element, true);
            if (!element.Exists(ownArchive))
            {
                element.Copy(GetArchiveAbsolutePath(element, false), ownArchive);
            }
        }

        public Element.ElementState IsElementInCorrectState(Element element, bool forceUpdate)
        {
            return element.GetStateForConfig(this, forceUpdate);
        }
    }
}