using UnityEngine;

public abstract class BaseAssetGenerator<T> : ScriptableObject where T : UnityEngine.Object
{
    [SerializeField, HideInInspector]
    protected T _asset;
    public T Asset{get {return _asset;}}

#if UNITY_EDITOR
    void OnEnable()
    {
        UnityEditor.EditorApplication.update = WaitUntilObjectCreation;
    }

    void WaitUntilObjectCreation()
    {
        if (string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(this))) return;
        UnityEditor.EditorApplication.update = null;
        OnValidate();
    }

    void OnValidate()
    {
        if (_asset == null)
        {
            if (string.IsNullOrEmpty(UnityEditor.AssetDatabase.GetAssetPath(this))) return;
            _asset = CreateAsset();
            UnityEditor.AssetDatabase.AddObjectToAsset(_asset, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
        }
        _asset.name = name;
        ProcessAsset(_asset);

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(_asset);
    }
#endif


    protected abstract T CreateAsset();
    protected abstract void ProcessAsset(T asset);
}


public abstract class BaseTextureGenerator : BaseAssetGenerator<Texture2D>
{
    protected sealed override Texture2D CreateAsset()
    {
        int width, height;
        TextureFormat format;
        bool useMipMap;
        GetParameters(out width, out height, out format, out useMipMap);
        return new Texture2D(width, height, format, useMipMap);
    }

    protected sealed override void ProcessAsset(Texture2D texture)
    {
        int width, height;
        TextureFormat format;
        bool useMipMap;
        GetParameters(out width, out height, out format, out useMipMap);

        if (texture.width != width || texture.height != height || texture.format != format || (texture.mipmapCount != 1) != useMipMap)
        {
            texture.Resize(width, height, format, useMipMap);
        }
        FillTexture(texture);
    }

    protected abstract void GetParameters(out int width, out int height, out TextureFormat format, out bool useMipMap);
    protected abstract void FillTexture(Texture2D texture);
}
