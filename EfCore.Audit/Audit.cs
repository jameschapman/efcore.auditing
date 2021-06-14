using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfCore.Audit
{
    public class Audit
    {
        /// <summary>The primary key</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        ///     The source row id saved as Json
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string RowId { get; set; }

        /// <summary>
        ///     The name of the table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Json containing information about the changes applied to the table record
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string Data { get; set; }

        /// <summary>
        ///     The type of change made, e.g. Added, Updated, Deleted
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        ///     The datetime this record was created
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     Unique identifier that groups all table changes for a given SaveChangesAsyncWithHistory call
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        ///     Who performed the action
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        ///     The client application through which the API was called
        /// </summary>
        public string Client { get; set; }
    }
}