//https://github.com/lyuma/LyumaShader/tree/dev/ShaderTools

#region
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
#endregion
namespace _3.Editor
{
    public class StencilInjector
    {
        public static readonly string[] Properties =
        {
            "[IntRange] _Stencil (\"Reference Value\", Range(0, 255)) = 0",
            "[IntRange] _StencilWriteMask (\"ReadMask\", Range(0, 255)) = 255",
            "[IntRange] _StencilReadMask (\"WriteMask\", Range(0, 255)) = 255",
            "[WideEnum(UnityEngine.Rendering.CompareFunction)] _StencilComp (\"Compare Function\", Int) = 8",
            "[WideEnum(UnityEngine.Rendering.StencilOp)] _StencilPass (\"Pass Op\", Int) = 0",
            "[WideEnum(UnityEngine.Rendering.StencilOp)] _StencilFail (\"Fail Op\", Int) = 0",
            "[WideEnum(UnityEngine.Rendering.StencilOp)] _StencilZFail (\"ZFail Op\", Int) = 0"
        };

        public static readonly string[] Pass =
        {
            "Stencil",
            "{",
            "Ref [_Stencil]",
            "ReadMask [_StencilReadMask]",
            "WriteMask [_StencilWriteMask]",
            "Comp [_StencilComp]",
            "Pass [_StencilPass]",
            "Fail [_StencilFail]",
            "ZFail [_StencilZFail]",
            /*"CompBack [_StencilCompBack]",
            "PassBack [_StencilPassBack]",
            "FailBack [_StencilFailBack]",
            "ZFailBack [_StencilZFailBack]",
            "CompFront [_StencilCompFront]",
            "PassFront [_StencilPassFront]",
            "FailFront [_StencilFailFront]",
            "ZFailFront [_StencilZFailFront]",*/
            "}"
        };

        public class StencilOperation : ShaderEditor.IShaderOperation
        {

            public string GetSuffix()
            {
                return "_stencil";
            }
            public bool ModifyShaderLines(ShaderEditor.ShaderState ss)
            {
                if (ss.editShaderNameLineNum == -1)
                {
                    EditorUtility.DisplayDialog("StencilInjector", "In " + ss.shaderName + ": failed to find Shader \"...\" block.", "OK", "");
                    // Failed to parse shader;
                    return false;
                }
                if (ss.endPropertiesLineNum == -1)
                {
                    EditorUtility.DisplayDialog("StencilInjector", "In " + ss.shaderName + ": failed to find end of Properties block.", "OK", "");
                    // Failed to parse shader;
                    return false;
                }
                if (ss.cgIncludeLineNum == -1)
                {
                    EditorUtility.DisplayDialog("StencilInjector", "In " + ss.shaderName + ": failed to find CGINCLUDE or appropriate insertion point.", "OK", "");
                    // Failed to parse shader;
                    return false;
                }
                int numSlashes = 0;
                if (!ss.path.StartsWith("Assets/", StringComparison.CurrentCulture))
                {
                    EditorUtility.DisplayDialog("StencilInjector", "Shader " + ss.shaderName + " at path " + ss.path + " must be in Assets!", "OK", "");
                    return false;
                }
                
                string includePrefix = "";
                Debug.Log("path is " + ss.path);
                foreach (char c in ss.path.Substring(7))
                {
                    if (c == '/')
                    {
                        numSlashes++;
                        includePrefix += "../";
                    }
                }
                if (ss.passBlockInjectionLine != -1)
                {
                    string passLine = ss.shaderData[ss.passBlockInjectionLine];
                    string passAdd = "\n" +
                        "       // Stencil Pass::\n" +
                        "       Stencil\n" +
                        "       {\n" +
                        "       Ref [_Stencil]\n" +
                        "       ReadMask [_StencilReadMask]\n" +
                        "       WriteMask [_StencilWriteMask]\n" +
                        "       Comp [_StencilComp]\n" +
                        "       Pass [_StencilPass]\n" +
                        "       Fail [_StencilFail]\n" +
                        "       ZFail [_StencilZFail]\n" +
                        "       }\n";
                    passLine = passAdd;
                    ss.shaderData[ss.passBlockInjectionLine] = passLine;
                }
                
                string epLine = ss.shaderData[ss.beginPropertiesLineNum];
                string propertiesAdd = "\n" +
                    "        // Stencil Properties::\n" +
                    "        [IntRange] _Stencil (\"Reference Value\", Range(0, 255)) = 0\n" +
                    "        [IntRange] _StencilWriteMask (\"ReadMask\", Range(0, 255)) = 255\n" +
                    "        [IntRange] _StencilReadMask (\"WriteMask\", Range(0, 255)) = 255\n" +
                    "        [WideEnum(UnityEngine.Rendering.CompareFunction)] _StencilComp (\"Compare Function\", Int) = 8\n" +
                    "        [WideEnum(UnityEngine.Rendering.StencilOp)] _StencilPass (\"Pass Op\", Int) = 0\n" +
                    "        [WideEnum(UnityEngine.Rendering.StencilOp)] _StencilFail (\"Fail Op\", Int) = 0\n" +
                    "        [WideEnum(UnityEngine.Rendering.StencilOp)] _StencilZFail (\"ZFail Op\", Int) = 0\n";
                epLine = epLine.Substring(0, ss.beginPropertiesSkip) + propertiesAdd + epLine.Substring(ss.beginPropertiesSkip);
                ss.shaderData[ss.beginPropertiesLineNum] = epLine;
                
                string shaderLine = ss.shaderData[ss.editShaderNameLineNum];
                shaderLine = shaderLine.Substring(0, ss.editShaderNameSkip) + ss.shaderSuffix + shaderLine.Substring(ss.editShaderNameSkip);
                ss.shaderData[ss.editShaderNameLineNum] = shaderLine;
                string prepend = "// AUTOGENERATED by StencilInjector at " + DateTime.UtcNow.ToString("s") + "!\n";
                prepend += ("// Original source file: " + ss.path + "\n");
                prepend += ("// This shader will not update automatically. Please regenerate if you change the original.\n");
                ss.shaderData[0] = prepend + ss.shaderData[0];
                for (int i = 0; i < ss.shaderData.Length; i++)
                {
                    if (ss.shaderData[i].IndexOf("CustomEditor", StringComparison.CurrentCulture) != -1)
                    {
                        ss.shaderData[i] = ("//" + ss.shaderData[i]);
                    }
                }
                return true;
            }
        }

        [MenuItem("Assets/Inject Stencils")]
        private static void InjectStencils()
        {
            Shader s = Selection.activeObject as Shader;
            Shader newShader = ShaderEditor.ModifyShader(s, new StencilOperation());
            Shader newS = newShader;
            EditorGUIUtility.PingObject(newS);
        }
    }
}
