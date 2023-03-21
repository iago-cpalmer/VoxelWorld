using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandPrompt : MonoBehaviour
{
    public InputField prompt;
    public bool active;

    List<string> hcmd = new List<string>();
    int historyCounter = -1;
    // Update is called once per frame
    private void Start()
    {
        Application.targetFrameRate = 1000;
        prompt = this.gameObject.GetComponent<InputField>();
        prompt.gameObject.GetComponent<Image>().color = new Color(prompt.gameObject.GetComponent<Image>().color.r, prompt.gameObject.GetComponent<Image>().color.g, prompt.gameObject.GetComponent<Image>().color.b, 0);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F2))
        {
            if(active)
            {
                prompt.text = "";
                active = false;
                prompt.DeactivateInputField();
                prompt.gameObject.GetComponent<Image>().color = new Color(prompt.gameObject.GetComponent<Image>().color.r, prompt.gameObject.GetComponent<Image>().color.g, prompt.gameObject.GetComponent<Image>().color.b, 0);
            } else
            {
                active = true;
                prompt.gameObject.GetComponent<Image>().color = new Color(prompt.gameObject.GetComponent<Image>().color.r, prompt.gameObject.GetComponent<Image>().color.g, prompt.gameObject.GetComponent<Image>().color.b, 1);
                prompt.ActivateInputField();
                historyCounter = -1;
            }

        }

        if(active )
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                executeCommand(getArgs(prompt.text));
                if (hcmd.Count >= 20)
                {
                    hcmd.RemoveAt(20);
                    
                }
                hcmd.Insert(0, prompt.text);

                prompt.text = "";
                active = false;
                prompt.DeactivateInputField();
                prompt.gameObject.GetComponent<Image>().color = new Color(prompt.gameObject.GetComponent<Image>().color.r, prompt.gameObject.GetComponent<Image>().color.g, prompt.gameObject.GetComponent<Image>().color.b, 0);
            } else if(Input.GetKeyDown(KeyCode.DownArrow)) { 

                    historyCounter--;
                    historyCounter = historyCounter % hcmd.Count;
                    prompt.text = hcmd[historyCounter];
            } else if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                    historyCounter++;
                    historyCounter = historyCounter % hcmd.Count;
                    prompt.text = hcmd[historyCounter];
                
                
            }
            
        }

    }



    string[] getArgs(string text) 
    {
        string[] args = new string[10];
        int argc = 0;
        for(int i = 0; i < text.Length;i++)
        {
            if(text[i] == ' ' || i==(text.Length-1))
            {
                
                if(text[i]==' ')
                {
                    args[argc] = text.Substring(0, i);
                } else
                {
                    args[argc] = text.Substring(0, i+1);
                }
                text = text.Substring(i+1, text.Length-(i+1));
                i = 0;
                argc++;
            }
        }
        string[] a = new string[argc];
        for (int i = 0; i < argc; i++)
        {
            a[i] = args[i];
        }
        return a;
       
    }

    void executeCommand(string[] args)
    {
        switch (args[0])
        {
            case "save":
                save(args);
                break;
            case "speed":
                speed(args);
                break;
            default:
                Debug.Log("Command not found");
                break;
        }
    }

    void save(string[] args)
    {
        Debug.Log("Saving world...");
        //save world
        WorldLoader.saveWorld();
        Debug.Log("World saved");

    }

    void speed(string[] args)
    {
        PlayerController.walkSpeed = Int32.Parse(args[1]);
    }
}
