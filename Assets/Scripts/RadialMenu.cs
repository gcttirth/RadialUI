using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public Button skillsButton;
    public List<Image> skillIcons;
    public float radius = 1.4f;
    public float animationSpeedNormalized = 1f;
    float scaleMultiplier = 1.5f; // Scale down the radius for the new arrangement
    float buttonScaleMultiplier = 1.5f; // Scale for main button
    [SerializeField]
    Transform openMenuPosHolder;
    Vector3 openMenuPosition;
    Vector3 closeMenuPosition;
    Vector3 baseSkillIconScale;

    int selectedSkillIndex = 3;
    int prevSelectedSkillIndex = 3;
    int topIconIndex = 0;
    int bottomIconIndex = 1;

    private bool isMenuOpen = false;

    [SerializeField]
    GameObject MenuBG;
    [SerializeField]
    CanvasGroup menuBase;
    [SerializeField]
    TextMeshProUGUI skillText;
    public bool isClickable = true;

    public float[] prevAngles;

    void Awake()
    {
        MenuBG.SetActive(false);
        menuBase.gameObject.SetActive(false);
        openMenuPosition = openMenuPosHolder.position;
        CalculateScaleFactor();
        prevAngles = new float[skillIcons.Count];
    }


    void Start()
    {
        baseSkillIconScale = skillIcons[0].transform.localScale;
        closeMenuPosition = skillsButton.transform.position;
        ArrangeIconsRadially();
        skillsButton.onClick.AddListener(OnSkillsButtonClick);

        foreach (Image icon in skillIcons)
        {
            icon.gameObject.GetComponent<Button>().onClick.AddListener(() => OnSkillIconClick(icon));
        }
    }


    void ArrangeIconsRadially(bool resetScale = false)
    {

        int numIcons = skillIcons.Count;
        for (int i = 0; i < numIcons; i++)
        {
            // Calculate the angle for each icon. Starting at the top (north) with 0 degrees.
            float angle = 90f - (i * 360f / numIcons);
            Vector3 newPosition = new Vector3(
                closeMenuPosition.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad),
                closeMenuPosition.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad),
                0f
            );
            prevAngles[i] = angle;
            if (resetScale)
            {
                Vector3 newScale = new Vector3(
                    skillIcons[i].transform.localScale.x * scaleMultiplier,
                    skillIcons[i].transform.localScale.y * scaleMultiplier,
                    skillIcons[i].transform.localScale.z
                );
                StartCoroutine(ScaleIcon(skillIcons[i], baseSkillIconScale, animationSpeedNormalized));
                //skillIcons[i].transform.localScale = newScale;
            }
            StartCoroutine(MoveIconToPosition(skillIcons[i], newPosition, animationSpeedNormalized));
            //skillIcons[i].transform.position = newPosition;
        }
    }

    void Update()
    {
        if (!isMenuOpen)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            MenuUp();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            MenuDown();
        }
        else if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        UpdateSkillText();
    }

    void OnSkillsButtonClick()
    {
        if (!isClickable)
        {
            return;
        }
        isClickable = false;
        isMenuOpen = !isMenuOpen;
        AnimateSkillsButton();
        if (isMenuOpen)
        {
            // Move the icon to the left side
            ToggleMenu(true);
            StartCoroutine(ScaleObject(skillsButton.gameObject, buttonScaleMultiplier, animationSpeedNormalized));
            StartCoroutine(MoveToPosition(skillsButton.gameObject, openMenuPosition, animationSpeedNormalized));
        }
        else
        {
            ToggleMenu(false);
            ArrangeIconsRadially(true);
            StartCoroutine(ScaleObject(skillsButton.gameObject, buttonScaleMultiplier, animationSpeedNormalized, false));
            StartCoroutine(MoveToPosition(skillsButton.gameObject, closeMenuPosition, animationSpeedNormalized));
        }
    }

    void OnSkillIconClick(Image clickedIcon)
    {
        if (!isMenuOpen || !isClickable)
        {
            return;
        }
        int clickedIndex = skillIcons.IndexOf(clickedIcon);
        int indexDiff = GetIndexDifference(clickedIndex, selectedSkillIndex, skillIcons.Count);
        if (indexDiff == 0)
        {
            return;
        }
        isClickable = false;
        if (isAfter(clickedIndex, selectedSkillIndex, skillIcons.Count))
        {
            MenuUp(indexDiff);
        }
        else
        {
            MenuDown(-indexDiff);
        }
        indexDiff = GetIndexDifference(clickedIndex, selectedSkillIndex, skillIcons.Count);
        UpdateSkillText();

        //selectedSkillIndex = clickedIndex;
    }

    void MenuUp(int stepCount = 1)
    {
        Debug.Log("Move up by " + stepCount);
        prevSelectedSkillIndex = selectedSkillIndex;
        selectedSkillIndex = (selectedSkillIndex + stepCount) % skillIcons.Count;
        RearrangeIcons(selectedSkillIndex, stepCount);
    }

    void MenuDown(int stepCount = -1)
    {
        Debug.Log("Move down by " + stepCount);
        prevSelectedSkillIndex = selectedSkillIndex;
        selectedSkillIndex = (selectedSkillIndex + stepCount + skillIcons.Count) % skillIcons.Count;
        RearrangeIcons(selectedSkillIndex, stepCount);
    }

    void AnimateSkillsButton()
    {
        // Potential animation for skill button here
    }

    void ToggleMenu(bool isOpen = true)
    {
        // For now, just reposition the icons to the left side
        RearrangeIcons(selectedSkillIndex);
        if (isOpen)
        {
            MenuBG.SetActive(isOpen);
        }
        StartCoroutine(AnimateMenu(isOpen));
    }

    IEnumerator AnimateMenu(bool fadeIn = true)
    {
        // Fade in the menu
        StartCoroutine(FadeInMenu(MenuBG.GetComponent<Image>(), animationSpeedNormalized, 0.5f, fadeIn));
        if (fadeIn)
        {
            yield return new WaitForSeconds(animationSpeedNormalized / 2);
        }
        StartCoroutine(FadeInMenu(menuBase, animationSpeedNormalized / 2, 1f, fadeIn));
    }

    IEnumerator FadeInMenu(Image image, float duration, float tillAlpha = 1, bool fadeIn = true)
    {
        float time = 0;
        float startAlpha = (fadeIn) ? 0 : tillAlpha;
        float endAlpha = (fadeIn) ? tillAlpha : 0;
        while (time < duration)
        {
            image.color = new Color(
                image.color.r,
                image.color.g,
                image.color.b,
                Mathf.Lerp(startAlpha, endAlpha, time / duration)
            );
            time += Time.deltaTime;
            yield return null;
        }
        if (!fadeIn)
        {
            image.gameObject.SetActive(false);
        }
    }
    IEnumerator FadeInMenu(CanvasGroup cg, float duration, float tillAlpha = 1, bool fadeIn = true)
    {
        if (fadeIn)
        {
            cg.gameObject.SetActive(true);
        }
        float time = 0;
        float startAlpha = (fadeIn) ? 0 : tillAlpha;
        float endAlpha = (fadeIn) ? tillAlpha : 0;
        while (time < duration)
        {
            // Fade in the menu
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        if (!fadeIn)
        {
            cg.gameObject.SetActive(false);
        }
        isClickable = true;
    }

    void RearrangeIcons(int selectedIconIndex, int direction = 0)
    {
        Debug.Log("Top: " + topIconIndex + ", Bottom: " + bottomIconIndex);
        float rearrangedRadius = radius * buttonScaleMultiplier; // Adjust radius for the new arrangement
        int numIcons = skillIcons.Count;

        float singleAngleOffset = 360f / 2f / (numIcons - 1); // Pre-calculate single angle offset

        for (int i = 0; i < numIcons; i++)
        {
            float indexDiff = GetIndexDifference(i, selectedIconIndex, numIcons);
            float angle = CalculateAngle(i, selectedIconIndex, indexDiff, singleAngleOffset);
            Vector3 newPosition = CalculateNewPosition(angle, rearrangedRadius);

            float _scaleMultiplier = CalculateScaleMultiplier(indexDiff);
            Vector3 newScale = new Vector3(
                baseSkillIconScale.x / _scaleMultiplier,
                baseSkillIconScale.y / _scaleMultiplier,
                skillIcons[i].transform.localScale.z
            );

            StartCoroutine(ScaleIcon(skillIcons[i], newScale, animationSpeedNormalized));

            bool shouldMoveBetweenAngles = (direction == 1 && bottomIconIndex == i) ||
                                           (direction == -1 && topIconIndex == i) ||
                                           (direction == 2 && !isAfter(i, prevSelectedSkillIndex, numIcons) && i != prevSelectedSkillIndex) ||
                                           (direction == -2 && isAfter(i, prevSelectedSkillIndex, numIcons) && i != prevSelectedSkillIndex);

            if (shouldMoveBetweenAngles)
            {
                StartCoroutine(MoveIconBetweenAngles(skillIcons[i], prevAngles[i], angle, animationSpeedNormalized, rearrangedRadius, direction));
            }
            else
            {
                StartCoroutine(MoveIconToPosition(skillIcons[i], newPosition, animationSpeedNormalized));
            }

            prevAngles[i] = angle;
        }

        topIconIndex = (topIconIndex + direction + numIcons) % numIcons;
        bottomIconIndex = (bottomIconIndex + direction + numIcons) % numIcons;
    }

    float CalculateAngle(int index, int selectedIndex, float indexDiff, float singleAngleOffset)
    {
        if (index == selectedIndex)
        {
            return 180f; // Selected icon at the west
        }
        else
        {
            float offsetAngle = indexDiff * singleAngleOffset;
            if (isAfter(index, selectedIndex, skillIcons.Count))
            {
                offsetAngle = -offsetAngle;
            }
            return 180f + offsetAngle;
        }
    }

    Vector3 CalculateNewPosition(float angle, float radius)
    {
        return new Vector3(
            openMenuPosition.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad),
            openMenuPosition.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad),
            0f
        );
    }

    float CalculateScaleMultiplier(float indexDiff)
    {
        if (indexDiff == 0)
        {
            return 1.15f;
        }
        return 1f + 0.4f * indexDiff;
    }

    IEnumerator MoveIconToPosition(Image icon, Vector3 targetPosition, float duration)
    {
        yield return StartCoroutine(MoveCoroutine(icon.transform, targetPosition, duration));
    }

    IEnumerator MoveToPosition(GameObject icon, Vector3 targetPosition, float duration)
    {
        yield return StartCoroutine(MoveCoroutine(icon.transform, targetPosition, duration));
    }

    IEnumerator MoveCoroutine(Transform transform, Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    IEnumerator MoveIconBetweenAngles(Image icon, float startAngle, float endAngle, float duration, float radius, int direction)
    {
        float time = 0;
        Vector3 startPosition = icon.transform.position;
        endAngle = startAngle;
        // Calculate endAngle
        switch (direction)
        {
            case 1:
                endAngle += +180;
                break;
            case -1:
                endAngle += -180;
                break;
            case 2:
                endAngle += +180 + 45;
                break;
            case -2:
                endAngle += -180 - 45;
                break;
            default:
                endAngle += 0;
                break;
        }
        Debug.Log("Moving icon index " + skillIcons.IndexOf(icon) + " from " + startAngle + " to " + endAngle);
        while (time < duration)
        {
            // Instead of linear interpolation, use circular interpolation
            float angle = Mathf.Lerp(startAngle, endAngle, time / duration);
            Vector3 newPosition = new Vector3(
                openMenuPosition.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad),
                openMenuPosition.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad),
                0f
            );
            icon.transform.position = newPosition;
            time += Time.deltaTime;
            yield return null;
        }
        //icon.transform.position = startPosition;
        isClickable = true;
    }

    IEnumerator ScaleIcon(Image icon, Vector3 targetScale, float duration)
    {
        yield return StartCoroutine(ScaleCoroutine(icon.transform, targetScale, duration));
    }
    IEnumerator ScaleObject(GameObject icon, float scaleMultiplier, float duration, bool isScaleUp = true)
    {
        Vector3 startScale = icon.transform.localScale;
        Vector3 targetScale = (isScaleUp) ?
            new Vector3(startScale.x * scaleMultiplier, startScale.y * scaleMultiplier, startScale.z) :
            new Vector3(startScale.x / scaleMultiplier, startScale.y / scaleMultiplier, startScale.z);
        yield return StartCoroutine(ScaleCoroutine(icon.transform, targetScale, duration));
    }

    IEnumerator ScaleCoroutine(Transform transform, Vector3 targetScale, float duration)
    {
        float time = 0;
        Vector3 startScale = transform.localScale;
        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }

    int GetIndexDifference(int index1, int index2, int total)
    {
        return Mathf.Min(Mathf.Abs(index1 - index2), total - Mathf.Abs(index1 - index2));
    }

    bool isAfter(int index1, int selectedIndex, int total)
    {
        if (index1 > selectedIndex && index1 - selectedIndex <= total / 2)
        {
            return true;
        }
        else
        {
            int distance = selectedIndex - index1;
            if (distance > total / 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    void CalculateScaleFactor()
    {
        // Base resolution is 1920x1080
        float baseWidth = 1920f;
        float baseHeight = 1080f;

        // Get the current resolution
        float currentWidth = Screen.width;
        float currentHeight = Screen.height;

        // Calculate the scaling factor based on width and height ratios
        float widthScale = currentWidth / baseWidth;
        float heightScale = currentHeight / baseHeight;

        float scale = widthScale;
        if (scale < 1)
        {
            return;
        }
        radius /= heightScale;
        radius *= Mathf.Abs(1 - scale) / 2 + 1;
    }

    public void UpdateSkillText()
    {
        string text;
        switch (selectedSkillIndex)
        {
            case 0:
                text = "Light";
                break;
            case 1:
                text = "Dark";
                break;
            case 2:
                text = "Fire";
                break;
            case 3:
                text = "Water";
                break;
            case 4:
                text = "Nature";
                break;
            default:
                text = "Water";
                break;

        }
        skillText.text = text;
    }

}
