using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerInteraction : MonoBehaviour
{
    public Transform highlightBlock;
    public Transform placeBlock;

    public PlayerController pController;

    public float checkIncrement = 0.1f;
    public float reach = 4f;

    public Transform cam;
    public World world;

    public Text selectedBlockText;
    public byte selectedBlockIndex = 1;

    float timeBetweenInteraction = 0f;

    public bool isBreaking = false;
    public float timerToBreak;

    private Vector3 currentBlockToDestroy;
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName;
    }

    private void Update()
    {
        if(timeBetweenInteraction<0.2f)
            timeBetweenInteraction += Time.deltaTime;
        GetPlayerInput();
        placeCursorBlocks();



        //Break a block
        if(isBreaking && currentBlockToDestroy == highlightBlock.position)
        {
            timerToBreak += Time.deltaTime;
        } else
        {
            timerToBreak = 0f;
        }
        if(isBreaking && timerToBreak>0f)
        {
            breakBlock();
            timerToBreak = 0f;
        }
    }

    private void placeCursorBlocks()
    {

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos))
            {

                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }

    private void GetPlayerInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0)
        {
            if(scroll>0)
            {
                selectedBlockIndex++;
            } else
            {
                selectedBlockIndex--;
            }

            if(selectedBlockIndex > (byte)(world.blockTypes.Length -1)) {
                selectedBlockIndex = 1;
            } else if (selectedBlockIndex < 1)
            {
                selectedBlockIndex = (byte)(world.blockTypes.Length - 1);
            }
            selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName;
        }

        if(highlightBlock.gameObject.activeSelf)
        {
            //Destroy block
            if(Input.GetMouseButton(0) && (timeBetweenInteraction > 0.2f))
            {
                isBreaking = true;
                currentBlockToDestroy = highlightBlock.position;
                timeBetweenInteraction = 0f;
            } 
            if(Input.GetMouseButtonUp(0))
            {
                isBreaking = false;
                timerToBreak = 0;
            }
            //Place block
            if(Input.GetMouseButton(1)&&canPlace(placeBlock.position) && (timeBetweenInteraction > 0.2f))
            {

                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
                timeBetweenInteraction = 0f;
            }
        }
    }
    private bool canPlace(Vector3 blockPos)
    {
        int x = Mathf.FloorToInt(transform.position.x);
        int y = Mathf.FloorToInt(transform.position.y);
        int z = Mathf.FloorToInt(transform.position.z);

        if (blockPos.x == x && blockPos.z == z && (blockPos.y == y || blockPos.y == y + 1))
        {
            return false;
        } else
        {
            return true;
        }
    }

    private void breakBlock()
    {
        world.GetChunkFromVector3(highlightBlock.position).EditVoxel(currentBlockToDestroy, 0);
    }
 
}
