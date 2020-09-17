db = db.getSiblingDB("nineGag");

db.createCollection("locker");

db.createUser({
  user: "service",
  pwd: "service",
  roles: [{ role: "readWrite", db: "nineGag" }],
  mechanisms: ["SCRAM-SHA-256"]
});
