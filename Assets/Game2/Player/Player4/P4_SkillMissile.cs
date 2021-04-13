using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P4_SkillMissile : Missile_Script //継承
{
    public float growUpSpeed;

    new void Update()
    {
        base.Update();
        transform.localScale = new Vector3(
            this.transform.localScale.x + Time.deltaTime * growUpSpeed,
            this.transform.localScale.y + Time.deltaTime * growUpSpeed,
            this.transform.localScale.z + Time.deltaTime * growUpSpeed
            );
    }
}
