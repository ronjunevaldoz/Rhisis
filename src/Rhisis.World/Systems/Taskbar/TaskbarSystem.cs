﻿using Microsoft.Extensions.Logging;
using Rhisis.Core.Common;
using Rhisis.Core.Common.Game.Structures;
using Rhisis.Core.DependencyInjection;
using Rhisis.Database;
using Rhisis.Database.Entities;
using Rhisis.World.Game.Components;
using Rhisis.World.Game.Entities;
using System;
using System.Collections.Generic;

namespace Rhisis.World.Systems.Taskbar
{
    [Injectable]
    public sealed class TaskbarSystem : ITaskbarSystem
    {
        public const int MaxTaskbarApplets = 18;
        public const int MaxTaskbarItems = 9;
        public const int MaxTaskbarItemLevels = 8;
        public const int MaxTaskbarQueue = 5;

        private readonly ILogger<TaskbarSystem> _logger;
        private readonly IDatabase _database;

        /// <inheritdoc />
        public int Order => 1;

        /// <summary>
        /// Creates a new <see cref="TaskbarSystem"/> instance.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="database">Rhisis database access layer.</param>
        public TaskbarSystem(ILogger<TaskbarSystem> logger, IDatabase database)
        {
            this._logger = logger;
            this._database = database;
        }

        /// <inheritdoc />
        public void Initialize(IPlayerEntity player)
        {
            IEnumerable<DbShortcut> shortcuts = this._database.TaskbarShortcuts.GetAll(x => x.CharacterId == player.PlayerData.Id);

            player.Taskbar.Applets = new TaskbarAppletContainerComponent(MaxTaskbarApplets);
            player.Taskbar.Items = new TaskbarItemContainerComponent(MaxTaskbarItems, MaxTaskbarItemLevels);
            player.Taskbar.Queue = new TaskbarQueueContainerComponent(MaxTaskbarQueue);

            foreach (DbShortcut dbShortcut in shortcuts)
            {
                Shortcut shortcut = null;

                if (dbShortcut.Type == ShortcutType.Item)
                {
                    var item = player.Inventory.GetItem(x => x.Slot == dbShortcut.ObjectId);

                    shortcut = new Shortcut(dbShortcut.SlotIndex, dbShortcut.Type, (uint)item.UniqueId, dbShortcut.ObjectType, dbShortcut.ObjectIndex, dbShortcut.UserId, dbShortcut.ObjectData, dbShortcut.Text);
                }
                else
                {
                    shortcut = new Shortcut(dbShortcut.SlotIndex, dbShortcut.Type, dbShortcut.ObjectId, dbShortcut.ObjectType, dbShortcut.ObjectIndex, dbShortcut.UserId, dbShortcut.ObjectData, dbShortcut.Text);
                }

                if (dbShortcut.TargetTaskbar == ShortcutTaskbarTarget.Applet)
                {
                    this.AddApplet(player, shortcut);
                }
                else if (dbShortcut.TargetTaskbar == ShortcutTaskbarTarget.Item)
                {
                    this.AddItem(player, shortcut, dbShortcut.SlotLevelIndex.GetValueOrDefault(int.MaxValue));
                }
            }
        }

        /// <inheritdoc />
        public void Save(IPlayerEntity player)
        {
            DbCharacter character = this._database.Characters.Get(player.PlayerData.Id);

            character.TaskbarShortcuts.Clear();

            foreach (var applet in player.Taskbar.Applets.Objects)
            {
                if (applet == null)
                    continue;

                var dbApplet = new DbShortcut(ShortcutTaskbarTarget.Applet, applet.SlotIndex, applet.Type,
                    applet.ObjectId, applet.ObjectType, applet.ObjectIndex, applet.UserId, applet.ObjectData, applet.Text);

                if (applet.Type == ShortcutType.Item)
                {
                    var item = player.Inventory.GetItem((int)applet.ObjectId);
                    dbApplet.ObjectId = (uint)item.Slot;
                }

                character.TaskbarShortcuts.Add(dbApplet);
            }


            for (int slotLevel = 0; slotLevel < player.Taskbar.Items.Objects.Count; slotLevel++)
            {
                for (int slot = 0; slot < player.Taskbar.Items.Objects[slotLevel].Count; slot++)
                {
                    var itemShortcut = player.Taskbar.Items.Objects[slotLevel][slot];
                    if (itemShortcut == null)
                        continue;

                    var dbItem = new DbShortcut(ShortcutTaskbarTarget.Item, slotLevel, itemShortcut.SlotIndex,
                        itemShortcut.Type, itemShortcut.ObjectId, itemShortcut.ObjectType, itemShortcut.ObjectIndex,
                        itemShortcut.UserId, itemShortcut.ObjectData, itemShortcut.Text);

                    if (itemShortcut.Type == ShortcutType.Item)
                    {
                        var item = player.Inventory.GetItem((int)itemShortcut.ObjectId);
                        dbItem.ObjectId = (uint)item.Slot;
                    }

                    character.TaskbarShortcuts.Add(dbItem);
                }
            }

            foreach (var queueItem in player.Taskbar.Queue.Shortcuts)
            {
                if (queueItem == null)
                    continue;

                character.TaskbarShortcuts.Add(new DbShortcut(ShortcutTaskbarTarget.Queue, queueItem.SlotIndex,
                    queueItem.Type, queueItem.ObjectId, queueItem.ObjectType, queueItem.ObjectIndex, queueItem.UserId,
                    queueItem.ObjectData, queueItem.Text));
            }

            this._database.Complete();
        }

        /// <inheritdoc />
        public void AddApplet(IPlayerEntity player, Shortcut shortcutToAdd)
        {
            if (shortcutToAdd.SlotIndex < 0 || shortcutToAdd.SlotIndex >= MaxTaskbarApplets)
                return;

            if (player.Taskbar.Applets.Objects[shortcutToAdd.SlotIndex] != null)
                return;

            player.Taskbar.Applets.Objects[shortcutToAdd.SlotIndex] = shortcutToAdd;

            this._logger.LogDebug("Created Shortcut of type {0} on slot {1} for player {2} on the Applet Taskbar", Enum.GetName(typeof(ShortcutType), shortcutToAdd.Type), shortcutToAdd.SlotIndex, player.Object.Name);
        }

        /// <inheritdoc />
        public void AddItem(IPlayerEntity player, Shortcut shortcutToAdd, int slotLevelIndex)
        {
            if (slotLevelIndex < 0 || slotLevelIndex >= MaxTaskbarItemLevels)
                return;

            if (shortcutToAdd.SlotIndex < 0 || shortcutToAdd.SlotIndex >= MaxTaskbarItems)
                return;

            player.Taskbar.Items.Objects[slotLevelIndex][shortcutToAdd.SlotIndex] = shortcutToAdd;

            this._logger.LogDebug("Created Shortcut of type {0} on slot {1} for player {2} on the Item Taskbar", Enum.GetName(typeof(ShortcutType), shortcutToAdd.Type), shortcutToAdd.SlotIndex, player.Object.Name);
        }

        /// <inheritdoc />
        public void RemoveApplet(IPlayerEntity player, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxTaskbarApplets)
                return;

            player.Taskbar.Applets.Objects[slotIndex] = null;

            this._logger.LogDebug("Removed Shortcut on slot {0} of player {1} on the Applet Taskbar", slotIndex, player.Object.Name);
        }

        /// <inheritdoc />
        public void RemoveItem(IPlayerEntity player, int slotLevelIndex, int slotIndex)
        {
            if (slotLevelIndex < 0 || slotLevelIndex >= MaxTaskbarItemLevels)
                return;

            if (slotIndex < 0 || slotIndex >= MaxTaskbarItems)
                return;

            player.Taskbar.Items.Objects[slotLevelIndex][slotIndex] = null;

            this._logger.LogDebug($"Removed Shortcut on slot {slotLevelIndex}-{slotIndex} of player {player.Object.Name} on the Item Taskbar");
        }
    }
}