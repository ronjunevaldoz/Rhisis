using Rhisis.Core.Structures.Game.Quests;
using Rhisis.Network.Packets;
using Sylver.Network.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Rhisis.World.Game.Structures
{
    public class QuestInfo : IEquatable<QuestInfo>, IPacketSerializer
    {
        /// <summary>
        /// Gets the quest id.
        /// </summary>
        public int QuestId { get; }

        /// <summary>
        /// Gets the database quest id.
        /// </summary>
        public int? DatabaseQuestId { get; }

        /// <summary>
        /// Gets the character id.
        /// </summary>
        public int CharacterId { get; }

        /// <summary>
        /// Gets the quest script.
        /// </summary>
        public IQuestScript Script { get; }

        /// <summary>
        /// Gets or sets the quest state.
        /// </summary>
        public QuestStateType State { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the quest is finished.
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the quest is checked.
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets a value that indicate sif the quest is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the quest start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the number of monsters killed for this quest.
        /// </summary>
        public IDictionary<int, short> Monsters { get; set; }

        /// <summary>
        /// Gets or sets a value that indiciates if the patrol has been done.
        /// </summary>
        public bool IsPatrolDone { get; set; }

        /// <summary>
        /// Creates a new <see cref="QuestInfo"/> instance.
        /// </summary>
        /// <param name="questId">Quest id.</param>
        /// <param name="characterId">Character id.</param>
        /// <param name="script">Quest script.</param>
        public QuestInfo(int questId, int characterId, IQuestScript script)
            : this(questId, characterId, script, default)
        {
        }

        /// <summary>
        /// Creates a new <see cref="QuestInfo"/> instance.
        /// </summary>
        /// <param name="questId">Quest id.</param>
        /// <param name="characterId">Character id.</param>
        /// <param name="script">Quest script.</param>
        /// <param name="databaseQuestId">Database quest id.</param>
        public QuestInfo(int questId, int characterId, IQuestScript script, int? databaseQuestId)
        {
            this.QuestId = questId;
            this.CharacterId = characterId;
            this.Script = script;
            this.DatabaseQuestId = databaseQuestId;
            this.Monsters = new Dictionary<int, short>();
        }

        /// <inheritdoc />
        public void Serialize(INetPacketStream packet)
        {
            packet.Write<short>((short)this.State); // state
            packet.Write<short>(0); // time limit
            packet.Write((short)this.QuestId);

            packet.Write<short>(Monsters?.ElementAtOrDefault(0).Value ?? 0); // monster 1 killed
            packet.Write<short>(Monsters?.ElementAtOrDefault(1).Value ?? 0); // monster 2 killed
            packet.Write<byte>(Convert.ToByte(this.IsPatrolDone)); // patrol done
            packet.Write<byte>(0); // dialog done
        }

        /// <summary>
        /// Compares two <see cref="QuestInfo"/> instances.
        /// </summary>
        /// <param name="other">Other <see cref="QuestInfo"/> instance.</param>
        /// <returns></returns>
        public bool Equals([AllowNull] QuestInfo other) => QuestId == other?.QuestId && CharacterId == other?.CharacterId;
    }
}