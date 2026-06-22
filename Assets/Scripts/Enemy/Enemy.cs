using Runtime;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : Initializer
{
    [SerializeField] private MoveAbility moveAbility;
    [SerializeField] private AttackAbility attackAbility;
    [SerializeField] private ActiveWeapon activeWeapon;
    [SerializeField] private DetectorPlayerInCircle detector;

    public override void Init(CMSEntity model)
    {
        moveAbility.Init(model.Get<TagSpeed>());
        
    }
}
