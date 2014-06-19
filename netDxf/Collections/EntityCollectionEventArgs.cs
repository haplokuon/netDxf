using System;
using netDxf.Entities;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents the arguments thrown by the <c>EntityCollection</c> events.
    /// </summary>
    public class EntityCollectionEventArgs :
        EventArgs
    {
        #region private fields

        private readonly EntityObject item;
        private bool cancel;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>EntityCollectionEventArgs</c>.
        /// </summary>
        /// <param name="item">Item that is being added or removed from the collection.</param>
        public EntityCollectionEventArgs(EntityObject item)
        {
            this.item = item;
            this.cancel = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get the item that is being added or removed from the collection.
        /// </summary>
        public EntityObject Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets or sets if the operation must be canceled.
        /// </summary>
        /// <remarks>This property is used by the OnBeforeAdd and OnBeforeRemove events to cancel the add or remove operation.</remarks>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        #endregion
    }
}