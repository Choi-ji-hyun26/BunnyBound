// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlatformIgnore : MonoBehaviour
// {
//     // 사다리 위에서 아래로 내려오기
//     public Collider2D platformCollider;
//     void OnTriggerStay2D(Collider2D collision) 
//     {
//         if (collision.gameObject.tag == "Player")
//         {
//             Physics2D.IgnoreCollision(collision.GetComponent<Collider2D>(), platformCollider, true);
//         }   
//     }

//     void OnTriggerExit2D(Collider2D collision)
//     {
//         if (collision.gameObject.tag == "Player")
//         {
//             Physics2D.IgnoreCollision(collision.GetComponent<Collider2D>(), platformCollider, false);
//         } 
//     }
// }
