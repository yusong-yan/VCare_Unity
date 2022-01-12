 using UnityEngine;
 
 public class Cam : MonoBehaviour
 {
     public int Speed = 50;
 
     void Update()
     {
         float xAxisValue = Input.GetAxis("Horizontal") * Speed;
         float zAxisValue = Input.GetAxis("Vertical") * Speed;
 
         Vector3 moveDirection = new Vector3(xAxisValue, 0.0f, zAxisValue);
         transform.position += moveDirection;
     }
 }
