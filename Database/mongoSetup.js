db = db.getSiblingDB("nineGag");

db.createCollection("_locker");

db.createUser({
  user: "service",
  pwd: "service",
  roles: [{ role: "readWrite", db: "nineGag" }],
  mechanisms: ["SCRAM-SHA-256"]
});
