set IPFS_PATH=%1
set LIBP2P_FORCE_PNET=1
ipfs.exe init
ipfs bootstrap rm --all
ipfs bootstrap add /dns4/dtp.northeurope.cloudapp.azure.com/tcp/4001/ipfs/QmWn6NyGNRHet6x7SUfC1GCwvLB6iR1zZABkx84YVZ1ngJ