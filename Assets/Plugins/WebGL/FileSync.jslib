// Dans un navigateur, Application.persistentDataPath est un système de fichiers
// en mémoire, recopié dans IndexedDB. Sans synchronisation explicite, les catégories
// personnalisées pourraient être perdues en fermant l'onglet.
mergeInto(LibraryManager.library, {
  SyncPersistentFiles: function () {
    FS.syncfs(false, function (error) {
      if (error)
        console.warn("Sauvegarde des fichiers impossible : " + error);
    });
  }
});
