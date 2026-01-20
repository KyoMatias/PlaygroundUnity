using System;
using UnityEngine;

/* =============================================================================
   Project:        $PROJECT_NAME$
   File:           $NAME$.cs
   Author:         $USER$
   Studio:         SundayMood Studios (Indie Home Studio)
   IDE:            JetBrains Rider
   Engine:         Unity
   Created:        $DATE$

   Description:
   ---------------------------------------------------------------------------
   [Brief description of what this script does.]

   Notes:
   ---------------------------------------------------------------------------
   - Part of the $PROJECT_NAME$ project by SundayMood Studios.
   ========================================================================== */

public class GameManager : MonoBehaviour
{
   public static GameManager Instance { get; private set; }

   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }

      Instance = this;
      DontDestroyOnLoad(gameObject);
   }
}