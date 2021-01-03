const Migrations = artifacts.require("Migrations");
const MerkeleRootSubmit = artifacts.require("MerkeleRootSubmit");

module.exports = function (deployer) {
  // deployer.deploy(Migrations);
  deployer.deploy(MerkeleRootSubmit);
};
