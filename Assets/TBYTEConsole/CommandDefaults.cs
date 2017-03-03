using UnityEngine;
using UnityEngine.SceneManagement;
using TBYTEConsole;

public static class CommandDefaults
{
    static CommandDefaults()
    {
        ConsoleLocator.cmdRegistry.Register(new CCommand("changescene", ChangeSceneCommand));
    }

    static public string ChangeSceneCommand(string[] Arguments)
    {
        if(Arguments.Length < 1)
        {
            return "Please supply the name of the scene to switch to.";
        }

        var scene = SceneManager.GetSceneByName(Arguments[0]);
        if(!scene.IsValid())
        {
            return "Failed to find a scene named: " + Arguments[0];
        }


        if(Application.isEditor)
        {
            SceneManager.LoadScene(Arguments[0]);
        }
        else
        {
            SceneManager.LoadScene(Arguments[0]);
        }

        return "Scene changed.";
    } 
}