using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RaymarchCamera : SceneViewFilter
{
    public Shader shader;
    private Material raymarchMaterial;
    public Material RaymarchMaterial
    {
        get
        {
            if(raymarchMaterial == null && shader != null)
            {
                raymarchMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            return raymarchMaterial;
        }
    }

    private new Camera camera;
    public Camera Camera
    {
        get { return camera ?? (camera = GetComponent<Camera>()); }
    }

    public Transform directionalLight;

    public float maxDistance;
    public Vector3 spherePos;
    public float sphereRadius;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (RaymarchMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        RaymarchMaterial.SetVector("_lightDir", directionalLight ? directionalLight.forward : Vector3.down);
        RaymarchMaterial.SetMatrix("_CamFrustum", CamFrustum(Camera));
        RaymarchMaterial.SetMatrix("_CamToWorld", Camera.cameraToWorldMatrix);
        RaymarchMaterial.SetFloat("_maxDistance", maxDistance);
        RaymarchMaterial.SetVector("_sphere1", new Vector4(spherePos.x, spherePos.y, spherePos.z, sphereRadius));

        RenderTexture.active = destination;
        RaymarchMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        RaymarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        //BL 
        GL.MultiTexCoord2(0, 0f, 0f);
        GL.Vertex3(0f, 0f, 3f); // third arg corresponds to the row index of BL set in CamFrustum method, and is retrieved by the shader's vert function
        //BR
        GL.MultiTexCoord2(0, 1f, 0f);
        GL.Vertex3(1f, 0f, 2f);
        //TR 
        GL.MultiTexCoord2(0, 1f, 1f);
        GL.Vertex3(1f, 1f, 1f);
        //TL 
        GL.MultiTexCoord2(0, 0f, 1f);
        GL.Vertex3(0f, 1f, 0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CamFrustum(Camera cam)
    {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);

        Vector3 up = Vector3.up * fov;
        Vector3 right = Vector3.right * fov * cam.aspect;

        Vector3 TL = (-Vector3.forward - right + up);
        Vector3 TR = (-Vector3.forward + right + up);
        Vector3 BL = (-Vector3.forward - right - up);
        Vector3 BR = (-Vector3.forward + right - up);

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);

        return frustum;
    }
}
