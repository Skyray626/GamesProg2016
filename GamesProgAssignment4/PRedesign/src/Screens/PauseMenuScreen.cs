﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PRedesign
{
    class PauseMenuScreen : MenuScreen
    {

        #region Initialization
        public PauseMenuScreen() : base("Paused")
        {
            //Create the menu entries
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume Game");
            MenuEntry returnToMainMenu = new MenuEntry("Main Menu");
            MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");

            //Set up their handlers
            resumeGameMenuEntry.Selected += OnCancel;
            returnToMainMenu.Selected += OnMainMenuSelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            //Add them to the list of options
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(returnToMainMenu);
            MenuEntries.Add(quitGameMenuEntry);
        }
        #endregion

        #region Update and Draw
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.SpriteFont;

            //Fade any other screens to black a little
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            base.Draw(gameTime);
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Event handler for the quite game menu option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void QuitGameMenuEntrySelected(object sender, EventArgs e) {
            const string message = "Are you sure you want to quit the game?";
            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);
            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmQuitMessageBox);
        }

        //Event handler for the message box to quite the game and return to the main menu
        void ConfirmQuitMessageBoxAccepted(object sender, EventArgs e) {
            ObjectManager.clearAll();
            ScreenManager.Game.Exit();
        }

        /// <summary>
        /// Event handler for the return to main menu option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMainMenuSelected(object sender, EventArgs e) {
            const string message = "Are you sure you want to return to the main menu?";
            MessageBoxScreen confirmMainMenuMessageBox = new MessageBoxScreen(message);
            confirmMainMenuMessageBox.Accepted += ConfirmMainMenuMessageBoxAccepted;
            ScreenManager.AddScreen(confirmMainMenuMessageBox);
        }

        //Event handler for the message box to return to the main menu
        void ConfirmMainMenuMessageBoxAccepted(object sender, EventArgs e) {
            ObjectManager.clearAll();
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
        }
        #endregion
    }
}
