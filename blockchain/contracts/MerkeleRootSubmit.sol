// SPDX-License-Identifier: MIT
pragma solidity >=0.4.22 <0.8.0;

contract MerkeleRootSubmit {
    string public rootHash;

    constructor(string memory _rootHash) public{
        rootHash = _rootHash;
    }
    //
    function insert(string memory userRootHash) public{
        rootHash = userRootHash;
    }
    //
    function storeBlockChain() public{
        //what to do here?!
    }

    function getRoot() public returns(string memory){
        return rootHash;
    }

    function verify(string memory inputMerkleRoot) public returns(bool){
        return compareStrings(inputMerkleRoot,rootHash);
    }

    function compareStrings (string memory a, string memory b)  internal pure returns (bool){
       return (keccak256(abi.encodePacked((a))) == keccak256(abi.encodePacked((b))));
   }

}
