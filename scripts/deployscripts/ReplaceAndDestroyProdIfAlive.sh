if python3 MinitwitAlive.py $1 $2; then
        if python3 reassign_floating_ip.py $1; then
            echo Destroying droplet $3
            vagrant destroy $3
            if python3 rename_droplet.py $1 $3 $2; then
                echo rename success
            else 
                echo rename fail
            fi
        else 
            echo droplet $1 not found
            vagrant destroy $1
        fi
else
    echo droplet $1 not alive
    vagrant destroy $1
fi