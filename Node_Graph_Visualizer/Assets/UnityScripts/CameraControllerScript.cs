using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{

  public float speedH = 1.0f;
  public float speedV = 1.0f;

  private float yaw = 0.0f;
  private float pitch = 0.0f;

  private float rightClick = 0.0f;

  CursorLockMode lockMode;
   // Start is called before the first frame update
   void Start()
   {
       //QualitySettings.vSyncCount = 0;
       //Application.targetFrameRate = 10;
   }
   // Update is called once per frame
   void Update()
   {
      float horizontal = Input.GetAxis("Horizontal");
      float vertical = Input.GetAxis("Vertical");


      transform.position = transform.position + transform.forward * 10f * vertical * Time.deltaTime;

      yaw += speedH * Input.GetAxis("Mouse X");
      pitch -= speedV * Input.GetAxis("Mouse Y");

      rightClick = Input.GetAxis("Fire2");

      if (rightClick == 1.0f)
      {
          lockMode = CursorLockMode.Locked;
          Cursor.lockState = lockMode;
          Vector3 position = transform.position;
          transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
          position += transform.right * 10f * horizontal * Time.deltaTime;
          transform.position = position;
      }
      else
      {
        lockMode = CursorLockMode.None;
        Cursor.lockState = lockMode;
      }
   }

 void Awake () {}
}
