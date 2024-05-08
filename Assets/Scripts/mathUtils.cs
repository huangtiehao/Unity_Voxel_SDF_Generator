using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class myMathUtil
    {
        public static Matrix4x4 setOrthoGraphicProjection(float left, float right, float bottom, float top, float near, float far) 
        {
            Matrix4x4 projectionMatrix= Matrix4x4.identity;
            projectionMatrix[0,0] = 2.0f / (right - left);
            projectionMatrix[1,1] = 2.0f / (bottom - top);
            projectionMatrix[2,2] = 1.0f / (far - near);
            projectionMatrix[3,0] = -(right + left) / (right - left);
            projectionMatrix[3,1] = -(bottom + top) / (bottom - top);
            projectionMatrix[3,2] = -near / (far - near);
            return projectionMatrix;
        }
        
        public static Matrix4x4 setViewDirection(Vector3 position, Vector3 direction, Vector3 up)
        {
            Matrix4x4 viewMatrix = Matrix4x4.identity;
            Vector3 w=Vector3.Normalize(direction) ;
            Vector3 u=Vector3.Normalize(Vector3.Cross(w, up));
            Vector3 v=Vector3.Cross(w, u);
            
            viewMatrix[0,0] = u.x;
            viewMatrix[1,0] = u.y;
            viewMatrix[2,0] = u.z;
            viewMatrix[0,1] = v.x;
            viewMatrix[1,1] = v.y;
            viewMatrix[2,1] = v.z;
            viewMatrix[0,2] = w.x;
            viewMatrix[1,2] = w.y;
            viewMatrix[2,2] = w.z;
            viewMatrix[3,0] = -Vector3.Dot(u, position);
            viewMatrix[3,1] = -Vector3.Dot(v, position);
            viewMatrix[3,2] = -Vector3.Dot(w, position);
            return viewMatrix;
        }
    }
}