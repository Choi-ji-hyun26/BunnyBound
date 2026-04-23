using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;
using TMPro;

public class FeverUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feverText;

    public void ActivateFeverMode() // public : playermove에서 호출, 무적 타임
    {
        StartCoroutine(FeverCoroutine());
    }
    private IEnumerator FeverCoroutine()
    {
        int feverDuration = 7;

        feverText.gameObject.SetActive(true);

        for (int i = feverDuration; i > 0; i--)
        {
            feverText.text = $"FEVER {i}";
            yield return new WaitForSeconds(1f);
        }

        feverText.text = "";
        feverText.gameObject.SetActive(false);
    }
}
