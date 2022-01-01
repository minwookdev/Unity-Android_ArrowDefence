﻿namespace ActionCat {
    using ActionCat.Interface;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class ArrowSkill {
        protected Transform arrowTr;
        protected Rigidbody2D rBody;
        protected IArrowObject arrow;

        /// <summary>
        /// Arrow classes that use skill must use init.
        /// </summary>
        /// <param name="tr">Arrow Transform</param>
        /// <param name="rigid">Arrow Rigid Body 2D</param>
        /// <param name="arrowInter">Interface Arrow Object</param>
        public virtual void Init(Transform tr, Rigidbody2D rigid, IArrowObject arrowInter)
        {
            arrowTr = tr;
            rBody   = rigid;
            arrow   = arrowInter;
        }

        public abstract void Clear();
    }

    public abstract class AttackActiveTypeAS : ArrowSkill {
        protected GameObject lastHitTarget = null;

        public abstract bool OnHit(Collider2D target, ref DamageStruct damage, Vector3 contact, Vector2 direction);

        public virtual bool OnHit(Collider2D target, out Transform targetTr, ref DamageStruct damage, Vector3 contact, Vector2 direction) {
            targetTr = null; return true;
        }

        public virtual void OnExit(Collider2D target) {
            //Hit처리 되면서 대상 객체가 비활성화 처리됨과 동시에 Exit함수가 들어오면 NULL잡음.
            //if (target == null) return;

            //저장된 마지막 타격 객체 제거
            //if (target != null && target.gameObject == lastHitTarget) {
            //    lastHitTarget = null;
            //}

            //개선식
            if (target != null) {
                if (target.gameObject == lastHitTarget) {
                    lastHitTarget = null;
                }
                else return;
            }
            else return;
        }
    }

    public abstract class AirActiveTypeAS : ArrowSkill {
        public virtual void OnHitCallback(Transform tr) { }

        public abstract void OnUpdate();

        public virtual void OnFixedUpdate() { }
    }

    public abstract class AddProjTypeAS : ArrowSkill {
        public abstract void OnHit();
    }

    public class ReboundArrow : AttackActiveTypeAS {
        //Save Variables
        int maxChainCount = 2;  // Max Chain Count
        float scanRange   = 5f; // Monster Detect Range

        //Temp Variables
        List<Collider2D> tempCollList = null;
        int currentChainCount         = 0;  // Current Chain Count

        public override bool OnHit(Collider2D target, ref DamageStruct damage, Vector3 contact, Vector2 direction) {
            //=============================================[ PHASE I. ACTIVATING & TARGET CHECKER ]=========================================================
            //최근에 Hit처리한 객체와 동일한 객체와 다시 충돌될 경우, return 처리
            //해당 Monster Object를 무시함 [같은 객체에게 스킬 효과를 터트릴 수 없음]
            if (lastHitTarget == target.gameObject) {    // Same Target Check
                return false;                            // Ignore
            }
            else {
                // Max Chain : Try On Hit and Disable
                if (currentChainCount >= maxChainCount) { //Try OnHit
                    return target.GetComponent<IDamageable>().OnHitWithResult(ref damage, contact, direction);
                }

                // Not Max Chain : Try Activate Skill
                if (target.GetComponent<IDamageable>().OnHitWithResult(ref damage, contact, direction)) { //Try OnHit
                    currentChainCount++;
                    lastHitTarget = target.gameObject;
                }
                else { //if Failed OnHit, Ignore
                    return false;
                }
            }
            //==============================================================================================================================================

            //================================================[ PHASE II. REBOUND TARGET FINDER ]===========================================================
            tempCollList = new List<Collider2D>(Physics2D.OverlapCircleAll(arrowTr.position, scanRange, 1 << LayerMask.NameToLayer(AD_Data.LAYER_MONSTER)));
            var duplicateTarget = tempCollList.Find(element => element == target);
            if (duplicateTarget != null) tempCollList.Remove(duplicateTarget);  // Remove Duplicate Target
            if (tempCollList.Count <= 0) return true;                           // Rebound Target Not Found -> Disable.
            //==============================================================================================================================================

            //==============================================[ PHASE III. CALCULATE TARGET'S DISTANCE ]======================================================
            //Transform bestTargetTr = null;       // Used Transform is Instability
            Vector3 monsterPos     = Vector3.zero; // Transform이 아닌 Vector Type 사용.
            float closestDistSqr   = Mathf.Infinity;
            for (int i = 0; i < tempCollList.Count; i++) {
                Vector2 directionToTarget = tempCollList[i].transform.position - arrowTr.position;
                float distSqr = directionToTarget.sqrMagnitude;
                if(distSqr < closestDistSqr) {
                    closestDistSqr = distSqr;
                    monsterPos     = tempCollList[i].transform.position;
                }
            }
            //==============================================================================================================================================

            //================================================[ PHASE IV. FORCE TO TARGET POSITION ]========================================================
            arrow.ForceToTarget(monsterPos);
            return false;
            //==============================================================================================================================================
        }

        /// <summary>
        /// linked to air skills
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetTr"></param>
        /// <returns></returns>
        public override bool OnHit(Collider2D target, out Transform targetTr, ref DamageStruct damage, Vector3 contact, Vector2 direction) {
            //=============================================[ PHASE I. ACTIVATING & TARGET CHECKER ]=========================================================
            if (lastHitTarget == target.gameObject) {
                //Ignore Same Target for Collision Stay
                targetTr = null; return false;
            }
            else {
                // Max Chain : Try On Hit
                if (currentChainCount >= maxChainCount){
                    targetTr = null; 
                    return target.GetComponent<IDamageable>().OnHitWithResult(ref damage, contact, direction);
                }

                // Not Max Chain : Try OnHit and Activating Skill
                if (target.GetComponent<IDamageable>().OnHitWithResult(ref damage, contact, direction)) {
                    currentChainCount++;
                    lastHitTarget = target.gameObject;
                }
                else{ //if Failed OnHit, Ignore
                    targetTr = null;
                    return false;
                }
            }
            //==============================================================================================================================================

            //================================================[ PHASE II. REBOUND TARGET FINDER ]===========================================================
            tempCollList = new List<Collider2D>(Physics2D.OverlapCircleAll(arrowTr.position, scanRange, 1 << LayerMask.NameToLayer(AD_Data.LAYER_MONSTER)));
            var dupTarget = tempCollList.Find(element => element == target);
            if (dupTarget != null) tempCollList.Remove(dupTarget);  //Remove Duplicate Target Monster
            if (tempCollList.Count <= 0) {                          //Not Found Target -> Disable
                targetTr = null;
                return true;
            }
            //==============================================================================================================================================

            //==============================================[ PHASE III. CALCULATE TARGET'S DISTANCE ]======================================================
            Transform tempTransform = null;
            Vector3 monsterPos      = Vector3.zero; //Rebound Target Position Save.
            float closestDistSqr    = Mathf.Infinity;
            for (int i = 0; i < tempCollList.Count; i++) {
                Vector2 directionToTarget = tempCollList[i].transform.position - arrowTr.position;
                float distSqr = directionToTarget.sqrMagnitude;
                if (distSqr < closestDistSqr) {
                    closestDistSqr = distSqr;
                    monsterPos     = tempCollList[i].transform.position;
                    tempTransform  = tempCollList[i].transform; //Sending Target Transform.
                }
            }
            //==============================================================================================================================================

            //================================================[ PHASE IV. FORCE TO TARGET POSITION ]========================================================
            arrow.ForceToTarget(monsterPos);
            targetTr = tempTransform;
            return false;
            //==============================================================================================================================================
        }

        /// <summary>
        /// call when arrow disable
        /// </summary>
        public override void Clear() {
            lastHitTarget     = null;
            currentChainCount = 0;
        }

        /// <summary>
        /// Copy Class Constructor
        /// </summary>
        /// <param name="origin"></param>
        public ReboundArrow(ReboundArrow origin)
        {
            maxChainCount = origin.maxChainCount;
            scanRange     = origin.scanRange;
        }

        /// <summary>
        /// Create Skill Class Data in Skill Scripable Object
        /// </summary>
        /// <param name="item"></param>
        public ReboundArrow(DataRebound item)
        {
            scanRange     = item.ScanRadius;
            maxChainCount = item.MaxChainCount;
        }

        /// <summary>
        /// Public Empty Constructor for ES3
        /// </summary>
        public ReboundArrow() {

        }
    }

    public class HomingArrow : AirActiveTypeAS {
        Transform targetTr      = null;    //temp Target Transform
        float currentSearchTime = 0f;      //Current Target Search Time
        bool isFindTarget       = false;
                                       
        //Target Colliders
        Collider2D[] colliders = null;
        bool isFixDirection    = false;

        //Saving Variables
        float searchInterval = .1f;  //Find Target Update Interval
        float scanRadius     = 3f;   //Detection Range
        float speed          = 6f;   //Target Chasing Speed Value
        float rotateSpeed    = 800f; //Target Chasing Rotate Speed Value

        //Call Every Frames
        public override void OnUpdate() {
            //================================================[ FIND A NEW TARGET ]=======================================================
            if (isFindTarget == false) { 
                //Finding a new Target Transform.
                //isFindTarget = false;

                //Update Target Find Interval
                currentSearchTime -= Time.deltaTime;
                if(currentSearchTime <= 0) {
                    //Target Search Interval
                    targetTr = SearchTarget(out isFindTarget);
                    currentSearchTime = searchInterval;
                }
            }
            //==================================================[ TARGET FOUND ]==========================================================
            else {
                //Check the Target GameObject is Alive
                if (targetTr == null || targetTr.gameObject.activeSelf == false) {
                    isFindTarget = false;
                }
            }
            //============================================================================================================================
        }

        public override void OnFixedUpdate() {
            if (isFindTarget == true) {
                Homing(targetTr);
            }
            else {
                DirectionFix();
            }
        }

        /// <summary>
        /// Link with Hit Type Skill
        /// </summary>
        /// <param name="tr">target transform</param>
        public override void OnHitCallback(Transform tr) {
            if (tr != null) {
                targetTr = tr;
            }
        }

        public override void Clear()
        {
            targetTr  = null;
            colliders = null;
            currentSearchTime = 0f;
            isFindTarget      = false;
        }

        Transform SearchTarget(out bool isTargetFind) {
            //================================================[ START TARGET FIND ]==========================================================
            colliders = Physics2D.OverlapCircleAll(arrowTr.position, scanRadius, 1 << LayerMask.NameToLayer(AD_Data.LAYER_MONSTER));
            //==================================================[ NO TARGET FIND ]==========================================================
            if (colliders.Length <= 0) { 
                isTargetFind = false;
                return null;
            }
            //=================================================[ ONE TARGET FIND ]==========================================================
            else if (colliders.Length == 1) {
                isTargetFind = true;
                return colliders[0].transform;
            }
            //================================================[ MORE TARGET FIND ]==========================================================
            else { 
                float closestDistSqr = Mathf.Infinity;
                Transform optimalTargetTr = null;
                //Check Disatance Comparison.
                for (int i = 0; i < colliders.Length; i++)
                {
                    //Distance Check
                    float distSqr = (colliders[i].transform.position - arrowTr.position).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        //Catch Best Monster Target Transform
                        optimalTargetTr = colliders[i].transform;
                        closestDistSqr = distSqr;
                    }
                }

                isTargetFind = true;
                return optimalTargetTr;
            }
            //==============================================================================================================================
        }

        void Homing(Transform targetTr) {
            if (targetTr == null) { //Target Transform Check (call safety)
                return;
            }

            Vector2 direction = (Vector2)targetTr.position - rBody.position;
            direction.Normalize(); //Only Direction

            //Only Used Z angle : 2D
            float rotateAmount = Vector3.Cross(direction, arrowTr.up).z;

            rBody.angularVelocity = -rotateAmount * rotateSpeed;

            //Force To Arrow Forward [Delete this]
            rBody.velocity = arrowTr.up * speed;

            //Set Fix Direction is false, for Non-Target.
            isFixDirection = false;
        }

        void DirectionFix() {
            //if Not Found the Target Object, Force to Directly Direction.
            if(isFixDirection == false) { //Stop Arrow Homing.
                if (rBody.angularVelocity > 0f || rBody.velocity.magnitude < 10f) { 
                    rBody.angularVelocity = 0f;
                    arrow.ForceToDirectly();
                } 
                isFixDirection = true;
            }
        }

        public override void Init(Transform tr, Rigidbody2D rigid, IArrowObject arrowInter)
        {
            base.Init(tr, rigid, arrowInter);
            currentSearchTime = searchInterval;
        }

        /// <summary>
        /// Copy Class Constructor
        /// </summary>
        /// <param name="guidanceArrow"></param>
        public HomingArrow(HomingArrow origin)
        {
            searchInterval = origin.searchInterval;
            scanRadius     = origin.scanRadius;
            speed          = origin.speed;
            rotateSpeed    = origin.rotateSpeed;
        }

        /// <summary>
        /// Create Skill Data in Skill Scriptable Object
        /// </summary>
        /// <param name="data"></param>
        public HomingArrow(DataHoming data)
        {
            searchInterval = data.TargetSearchInterval;
            scanRadius     = data.ScanRadius;
            speed          = data.HomingSpeed;
            rotateSpeed    = data.HomingRotateSpeed;
        }

        /// <summary>
        /// TEMP Constructor
        /// </summary>
        public HomingArrow() {

        }
    }

    public class SplitArrow : AddProjTypeAS
    {
        public override void Clear()
        {
            throw new System.NotImplementedException();
        }

        public override void OnHit()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Copy Class Constructor
        /// </summary>
        /// <param name="splitArrow"></param>
        public SplitArrow(SplitArrow splitArrow)
        {

        }
    }

    public class PiercingArrow : AttackActiveTypeAS
    {
        public byte currentChainCount = 0;
        public byte maxChainCount;

        float tempRadius = 5f;

        ///관통 횟수에 따른 데미지 감소효과 구현필요.

        /// <summary>
        /// return true == DisableArrow || false == aliveArrow
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override bool OnHit(Collider2D target, ref DamageStruct damage, Vector3 contactpoint, Vector2 direction)
        {
            if (lastHitTarget == target.gameObject) {
                //Ignore Duplicate Target
                return false;
            }
            else {
                if (currentChainCount >= maxChainCount) {
                    //Hit 처리 후 화살객체 Disable
                    target.GetComponent<IDamageable>().OnHitWithDirection(ref damage, contactpoint, direction);
                    return true;
                }

                //연쇄횟수 중첩 및 타겟 저장
                currentChainCount++;
                lastHitTarget = target.gameObject;

                //Monster Hit 처리
                target.GetComponent<IDamageable>().OnHitWithDirection(ref damage, contactpoint, direction);
            } return false;
        }

        /// <summary>
        /// Linked Air Type Skill
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetTr"></param>
        /// <returns></returns>
        public override bool OnHit(Collider2D target, out Transform targetTr, ref DamageStruct damage, Vector3 contactpoint, Vector2 direction)
        {
            if(lastHitTarget == target.gameObject) {
                //Ignore Duplicate Target
                targetTr = null; return false;
            }
            else {
                if(currentChainCount >= maxChainCount) {
                    //Hit처리 후 Arrow Object Disable 요청
                    target.GetComponent<IDamageable>().OnHitWithDirection(ref damage, contactpoint, direction);
                    targetTr = null; return true;
                }

                //연쇄 횟수 중첩 및 타겟 저장
                currentChainCount++;
                lastHitTarget = target.gameObject;

                target.GetComponent<IDamageable>().OnHitWithDirection(ref damage, contactpoint, direction);
            }

            //Air Skill과 연계된 경우, 주변의 Random Monster Target을 넘겨줌
            var collList = new List<Collider2D>(
            Physics2D.OverlapCircleAll(arrowTr.position, tempRadius,
                                       1 << LayerMask.NameToLayer(AD_Data.LAYER_MONSTER)));
            for (int i = collList.Count - 1; i >= 0 ; i--) {
                if (collList[i].gameObject == lastHitTarget)
                    collList.Remove(collList[i]);
            }

            if(collList.Count <= 0) {
                targetTr = null;
                return false;
            }
            else {
                targetTr = collList[Random.Range(0, collList.Count)].transform;
                return false;
            }
        }

        public override void Clear()
        {
            currentChainCount = 0;
        }

        public PiercingArrow(PiercingArrow origin)
        {
            maxChainCount = origin.maxChainCount;
        }

        public PiercingArrow(DataPiercing data)
        {
            maxChainCount = data.MaxChainCount;
        }

        /// <summary>
        /// Public Empty Constructor for ES3
        /// </summary>
        public PiercingArrow() { }
    }

}
