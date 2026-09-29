using UnityEngine;
using UnityEngine.InputSystem;

public class Playerattack : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public chatemplate Player;

    private chatemplate target;

    public GameObject AttackHover;

    public bool canAttack;

    void Start()
    {
        Player = GetComponent<chatemplate>();

    }

    // Update is called once per frame
    void Update()
    {
        if (AttackHover != null)
            AttackHover.SetActive(false);

        if (Mouse.current == null || Player == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            target = hit.collider.GetComponent<chatemplate>();
        else
            return;
        if (target == null || target == Player)
                return;

        int distance = RangeCheck.measureGrid(Player.transform.position, target.transform.position);
        if (distance > Player.hit_range) return;

        // attack check
        if (Mouse.current.leftButton.wasReleasedThisFrame) {
            if (!canAttack)
            {
                Debug.Log("Cannot attack");
                return;
            }
            canAttack = attack.Hit(Player, target);
            EventMap.PlayerAfterAttack(Player, target, canAttack);


        }
        //attack hover

        if (AttackHover != null)
        {
            AttackHover.transform.position = target.transform.position + new Vector3(0, 1, 0);
            AttackHover.SetActive(true);
        }


    }
}
