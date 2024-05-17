using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

//A tool to generate signed distance fields from Mesh assets.
public class SdfGenerator{
  private Mesh mesh = null;
  public Mesh Mesh {
    get {
      return mesh;
    }
    set {
      mesh = value;
    }
  }

  private int subMeshIndex = 0;
  public int SubMeshIndex {
    get {
      return subMeshIndex;
    }
    set {
      if (value < 0) {
        throw new System.IndexOutOfRangeException("SubMeshIndex must be >= 0");
      }
      subMeshIndex = value;
    }
  }

  private float padding = 0.2f;
  public float Padding {
    get {
      return padding;
    }
    set {
      if (value < 0f) {
        throw new System.ArgumentException("Padding must be >= 0");
      }
      padding = value;
    }
  }

  private int sdfResolution = 32;
  public int SdfResolution {
    get {
      return sdfResolution;
    }
    set {
      if (value < 1) {
        throw new System.ArgumentException("Resolution must be >= 1");
      }
      sdfResolution = value;
    }
  }

  private struct Triangle {
    public Vector3 a, b, c;
  }

  public Texture3D generate() {
    if (mesh == null) {
      throw new System.ArgumentException("Mesh must have been assigned");
    }

    // Create the voxel texture.
    Texture3D voxels = new Texture3D(sdfResolution, sdfResolution, sdfResolution, TextureFormat.RFloat, false);
    voxels.anisoLevel = 1;
    voxels.filterMode = FilterMode.Trilinear;
    voxels.wrapMode = TextureWrapMode.Clamp;

    // Get an array of pixels from the voxel texture, create a buffer to
    // hold them, and upload the pixels to the buffer.
    float[] pixelArray = new float[sdfResolution*sdfResolution*sdfResolution];
    ComputeBuffer pixelBuffer = new ComputeBuffer(pixelArray.Length, sizeof(float));
    pixelBuffer.SetData(pixelArray);
    
    // Get an array of triangles from the mesh.
    Vector3[] meshVertices = mesh.vertices;
    int[] meshTriangles = mesh.GetTriangles(subMeshIndex);
    Triangle[] triangleArray = new Triangle[meshTriangles.Length / 3];
    for (int t = 0; t < triangleArray.Length; t++) {
        triangleArray[t].a = meshVertices[meshTriangles[3 * t + 0]];  // - mesh.bounds.center;
        triangleArray[t].b = meshVertices[meshTriangles[3 * t + 1]];  // - mesh.bounds.center;
        triangleArray[t].c = meshVertices[meshTriangles[3 * t + 2]];  // - mesh.bounds.center;
    }

    // Create a buffer to hold the triangles, and upload them to the buffer.
    ComputeBuffer triangleBuffer = new ComputeBuffer(triangleArray.Length, sizeof(float) * 3 * 3);
    triangleBuffer.SetData(triangleArray);

    // Instantiate the compute shader from resources.
    ComputeShader compute = Resources.Load("Shaders/GenSdf") as ComputeShader;
    //ComputeShader compute = Resources.Load("Shaders/GenerateSdf") as ComputeShader;
    int kernel = compute.FindKernel("CSMain");

    // Upload the pixel buffer to the GPU.
    compute.SetBuffer(kernel, "pixelBuffer", pixelBuffer);
    compute.SetInt("pixelBufferSize", pixelArray.Length);

    // Upload the triangle buffer to the GPU.
    compute.SetBuffer(kernel, "triangleBuffer", triangleBuffer);
    compute.SetInt("triangleBufferSize", triangleArray.Length);

    // Calculate and upload the other necessary parameters.
    compute.SetInt("textureSize", sdfResolution);
    Vector3 minExtents = Vector3.zero;
    Vector3 maxExtents = Vector3.zero;
    foreach (Vector3 v in mesh.vertices) {
      for (int i = 0; i < 3; i++) {
        minExtents[i] = Mathf.Min(minExtents[i], v[i]);
        maxExtents[i] = Mathf.Max(maxExtents[i], v[i]);
      }
    }
    compute.SetVector("minExtents", minExtents - Vector3.one*padding);
    compute.SetVector("maxExtents", maxExtents + Vector3.one*padding);
    
    //sample random ray
    Vector3[] sampleDirs = new Vector3[64];
    for (int i = 0; i < 64; ++i)
    {
      sampleDirs[i]=Vector3.Normalize(Random.onUnitSphere);
    }
    ComputeBuffer sampleDirBuffer = new ComputeBuffer(sampleDirs.Length, sizeof(float)* 3);
    sampleDirBuffer.SetData(sampleDirs);
    compute.SetBuffer(kernel,"sampleDirs",sampleDirBuffer);
    
    triangleBuffer.SetData(triangleArray);
    
    // Compute the SDF.
    compute.Dispatch(kernel, pixelArray.Length / 256 + 1, 1, 1);

    // Destroy the compute shader and release the triangle buffer.
    triangleBuffer.Release();

    // Retrieve the pixel buffer and reapply it to the voxels texture.
    pixelBuffer.GetData(pixelArray);
    pixelBuffer.Release();
    voxels.SetPixelData(pixelArray, 0);
    voxels.Apply();
    // Return the voxel texture.
    return voxels;
  }
}
