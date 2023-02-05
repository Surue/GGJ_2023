// Old Skull Games
// Bernard Barthelemy
// Thursday, June 7, 2018

using System;
using System.Reflection;
using UnityEditor;

[InitializeOnLoad]
public class ReferenceRemovalProjectHook
{
    // ReSharper disable once UnusedMember.Local  (it's used by reflection)
    private static string ReplaceBoo(string name, string content)
    {
        const string references =
            "\r\n    <Reference Include=\"Boo.Lang\" />\r\n    <Reference Include=\"UnityScript.Lang\" />";
        return content.Replace(references, "");
    }

    static ReferenceRemovalProjectHook()
    {
        string typeName =
            "SyntaxTree.VisualStudio.Unity.Bridge.ProjectFilesGenerator," +
            " SyntaxTree.VisualStudio.Unity.Bridge, " +
            "Culture=neutral, " +
            "PublicKeyToken=null";

        Type projectFilesGeneratorType = Type.GetType(typeName);
        if(projectFilesGeneratorType == null)
        {
            return;
        }

        FieldInfo info = projectFilesGeneratorType.GetField("ProjectFileGeneration", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        if(info==null)
            return;
        
        Type fieldType = info.FieldType;
        Delegate myDelegate = Delegate.CreateDelegate(fieldType, typeof(ReferenceRemovalProjectHook), "ReplaceBoo");
        Delegate currentDelegate = (Delegate) info.GetValue(null);
        if(currentDelegate != null)
        {
            myDelegate = Delegate.Combine(currentDelegate, myDelegate);
        }

        info.SetValue(null, myDelegate);
    }
}
