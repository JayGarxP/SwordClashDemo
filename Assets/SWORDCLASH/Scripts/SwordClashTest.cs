using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SwordClash
{
    public class SwordClashTest : MonoBehaviour
    {

        public enum OscillationFunction { Sine, Cosine }

        private Vector2 startingPosition;
        private bool HitBySword;
        private Vector2 BounceVelocity;
        private float BounceTime = 0.27f;
        private float CurrentBounceTime;
        private Rigidbody2D Rgbd;


        // Use this for initialization
        void Start()
        {
            startingPosition = transform.position;
            HitBySword = false;
            CurrentBounceTime = 0.0f;
            Rgbd = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            // oscillate back and forth real smooth-like if not hit by anything
            if (GetComponent<Rigidbody2D>() != null && HitBySword == false)
            {
                Oscillate(OscillationFunction.Sine, 0.03f);
            }
        }


        void FixedUpdate()
        {
            if (HitBySword)
            {
                Rgbd.MovePosition(Rgbd.position + BounceVelocity * Time.fixedDeltaTime);
                CurrentBounceTime += Time.fixedDeltaTime;
                if (CurrentBounceTime >= BounceTime)
                {
                    HitBySword = false;
                    CurrentBounceTime = 0.0f;
                }
            }

        }

        // called when this GameObject collides with GameObject2.
        void OnTriggerEnter2D(Collider2D col)
        {
            //Debug.Log("GameObject1 collided with " + col.name);
            if (col.tag == "TentacleTip" && HitBySword == false)
            {
                HitBySword = true;
                // reflect from position of TT that hit you (works OK if hit head on, not so good when any other direction...)
                BounceVelocity = Vector2.Reflect(col.transform.localPosition, Vector2.up);
            }

        }


        private void Oscillate(OscillationFunction method, float scalar)
        {
            if (method == OscillationFunction.Sine)
            {
                // Rgbd.MovePosition(new Vector2(startingPosition.x, startingPosition.y + Mathf.Sin(Time.time) * scalar));
                Rgbd.MovePosition(new Vector2(Rgbd.position.x, Rgbd.position.y + Mathf.Sin(Time.time) * scalar));
            }
            else if (method == OscillationFunction.Cosine)
            {
                transform.position = new Vector3(Mathf.Cos(Time.time) * scalar, 0, 0);
            }

        }

    }
}