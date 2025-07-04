// Elimina todos los documentos donde Path es null
print('Eliminando documentos con Path: null...');
db.files.deleteMany({ Path: null });

// Encuentra y elimina duplicados, dejando solo uno por cada Path
print('Eliminando duplicados por Path...');
db.files.aggregate([
  { $group: { _id: "$Path", ids: { $addToSet: "$_id" }, count: { $sum: 1 } } },
  { $match: { count: { $gt: 1 } } }
]).forEach(function(doc) {
  doc.ids.shift(); // deja uno
  db.files.deleteMany({ _id: { $in: doc.ids } });
});

print('Limpieza completada.'); 