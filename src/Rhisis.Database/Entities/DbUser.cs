﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Rhisis.Database.Entities
{
    [Table("Users")]
    public class DbUser : DbEntity
    {
        /// <summary>
        /// Gets or sets the user's identification name.
        /// </summary>
        [Required]
        [Encrypted]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        [Required]
        [Encrypted]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Required]
        [Encrypted]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's creation date.
        /// </summary>
        [Column(TypeName = "DATETIME")]
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets or sets the last time the user logged in.
        /// </summary>
        [Column(TypeName = "DATETIME")]
        public DateTime LastConnectionTime { get; set; }
        
        /// <summary>
        /// Gets or sets the user's authority.
        /// </summary>
        [Required]
        public int Authority { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates if the user is deleted.
        /// </summary>
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets the total play time of this user in seconds.
        /// </summary>
        [NotMapped]
        public long PlayTime => this.Characters.Sum(x => x.PlayTime);

        /// <summary>
        /// Gets or sets the user's characters list.
        /// </summary>
        public ICollection<DbCharacter> Characters { get; set; }

        /// <summary>
        /// Creates a new <see cref="DbUser"/> instance.
        /// </summary>
        public DbUser()
        {
            this.CreatedAt = DateTime.UtcNow;
            this.Characters = new HashSet<DbCharacter>();
        }
    }
}
