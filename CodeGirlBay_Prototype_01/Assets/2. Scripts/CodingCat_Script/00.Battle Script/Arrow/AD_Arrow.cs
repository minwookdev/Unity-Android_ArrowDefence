﻿namespace CodingCat_Games
{
    using CodingCat_Scripts;
    using UnityEngine;

    public class AD_Arrow : MonoBehaviour
    {
        //The Left, Right Clamp Point for the Arrow.
        public Transform leftClampPoint, rightClampPoint;
        public Transform arrowChatchPoint;
        public TrailRenderer arrowTrail;

        [ReadOnly] public bool isLaunched;
        [HideInInspector]
        public float power;

        //Controll Arrow Position (Before Launched)
        private Vector3 arrowPosition;

        //Launch Power for the Arrow
        private float powerFactor = 2000;
        private Rigidbody2D rBody;
        private PolygonCollider2D polyCollider;

        private void Start()
        {
            //if (ReferenceEquals(rBody, null)) rBody = gameObject.GetComponent<Rigidbody2D
            //Initial Arrow Childs
            if (arrowChatchPoint == null) arrowChatchPoint = transform.GetChild(2);
            if (arrowTrail == null) arrowTrail = transform.GetChild(2).GetChild(0).GetComponent<TrailRenderer>();
            rBody = gameObject.GetComponent<Rigidbody2D>();
            rBody.gravityScale = 0f;

            if (polyCollider == null) polyCollider = transform.GetChild(0).GetComponent<PolygonCollider2D>();
            polyCollider.enabled = false;
        }

        private void Update()
        {
            if (!isLaunched)
            {
                ClampPosition();
                CalculatePower();
            }
        }

        private void OnDisable() => this.isLaunched = false;

        private void ClampPosition()
        {
            //Get the Current Position of the Arrow
            arrowPosition = transform.position;
            //Clamp the X Y position Between min and Max Points
            arrowPosition.x = Mathf.Clamp(arrowPosition.x, Mathf.Min(rightClampPoint.position.x, leftClampPoint.position.x),
                                                           Mathf.Max(rightClampPoint.position.x, leftClampPoint.position.x));
            arrowPosition.y = Mathf.Clamp(arrowPosition.y, Mathf.Min(rightClampPoint.position.y, leftClampPoint.position.y),
                                                           Mathf.Max(rightClampPoint.position.y, leftClampPoint.position.y));

            //Set new Position for the Arrow
            transform.position = arrowPosition;
        }

        private void CalculatePower()
        {
            this.power = Vector2.Distance(transform.position, rightClampPoint.position) * powerFactor;
        }

        public void ShotArrow(Vector2 force, Transform parent)
        {
            //부모바꿔준 상태에서 발사
            //발사되고 난 뒤에 SetParent로 Canvas의 Child로 바꿔주지 않으면 활 각도 돌릴때마다 자식으로 취급되서 날아가면서 화살각도가 휘어버린다
            //발사할 때는 보정 필요함 뒤에 false 붙이면 이상한 곳에서 날아감;
            transform.SetParent(parent);

            this.rBody.isKinematic = false;
            //this.rBody.gravityScale = 0;
            this.isLaunched = true;
            this.rBody.AddForce(force, ForceMode2D.Force);

            //발사할 때 Clear 해주지 않으면 전에 있던 잔상이 남는다
            arrowTrail.gameObject.SetActive(true);
            arrowTrail.Clear();

            //Poly Collider가 활성되는 순간 충돌 가능
            polyCollider.enabled = true;
        }

        #region PROPERTIES

        public void OnDisableCollider() => this.polyCollider.enabled = false;

        #endregion
    }
}
