using UnityEditor;
using UnityEngine;

class RenderTextureUtils {
    // [1]
    public static void SaveRTToFile(RenderTexture rt, string name) {

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();

        string path = "Assets/DebugOutput/" + name + ".png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
        Debug.Log("Saved to " + path);
    }

}

/*

[1] https://gist.github.com/krzys-h/76c518be0516fb1e94c7efbdcd028830

*/
